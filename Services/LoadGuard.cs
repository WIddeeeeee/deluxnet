using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace DeluxeNET.Services
{
    /// <summary>
    /// Load-management settings. Bound from the "Load:" section of
    /// appsettings.json. All limits are per this server instance.
    /// </summary>
    public class LoadOptions
    {
        /// <summary>Master switch. When false, the middleware passes through untouched.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Concurrent in-flight requests above this get a 503 + Retry-After.</summary>
        public int MaxConcurrentRequests { get; set; } = 500;

        /// <summary>Sliding 60s window limit per IP for general API traffic.</summary>
        public int GeneralRequestsPerMinute { get; set; } = 100;

        /// <summary>Stricter sliding 60s window per IP for auth endpoints (register/login/token).</summary>
        public int AuthRequestsPerMinute { get; set; } = 10;

        /// <summary>Requests with a larger Content-Length get a 413. Also applied to Kestrel itself.</summary>
        public long MaxRequestBodyBytes { get; set; } = 10L * 1024 * 1024; // 10 MB

        /// <summary>Requests slower than this are logged as warnings.</summary>
        public double SlowRequestThresholdSeconds { get; set; } = 2.0;
    }

    /// <summary>
    /// Thread-safe runtime counters shared by the load-guard middleware and
    /// the /api/health endpoint. Registered as a singleton.
    /// </summary>
    public sealed class LoadStats : IDisposable
    {
        private int _concurrent;
        private long _totalRequests;
        private long _rateLimited;
        private long _overloadRejections;
        private readonly ConcurrentQueue<DateTime> _requestTimes = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _hitsPerBucket = new();
        private readonly Timer _cleanupTimer;
        private readonly DateTime _startedAt = DateTime.UtcNow;
        private bool _disposed;

        public LoadStats()
        {
            // Every 5 minutes, drop per-IP buckets idle for 2+ minutes so the
            // dictionary can't grow without bound from one-off visitors.
            _cleanupTimer = new Timer(_ => PruneIdleBuckets(), null,
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public DateTime StartedAt => _startedAt;
        public int Concurrent => Volatile.Read(ref _concurrent);
        public long TotalRequests => Interlocked.Read(ref _totalRequests);
        public long RateLimited => Interlocked.Read(ref _rateLimited);
        public long OverloadRejections => Interlocked.Read(ref _overloadRejections);

        /// <summary>Call at request start. Returns the new concurrent count.</summary>
        public int BeginRequest()
        {
            Interlocked.Increment(ref _totalRequests);
            var now = DateTime.UtcNow;
            _requestTimes.Enqueue(now);
            PruneTimes(now);
            return Interlocked.Increment(ref _concurrent);
        }

        public void EndRequest() => Interlocked.Decrement(ref _concurrent);

        public void NoteOverloadReject() => Interlocked.Increment(ref _overloadRejections);

        /// <summary>Requests per second over the trailing 60 seconds.</summary>
        public double RequestsPerSecond()
        {
            PruneTimes(DateTime.UtcNow);
            return _requestTimes.Count / 60.0;
        }

        private void PruneTimes(DateTime now)
        {
            var cutoff = now.AddMinutes(-1);
            while (_requestTimes.TryPeek(out var t) && t < cutoff)
                _requestTimes.TryDequeue(out _);
        }

        /// <summary>
        /// Sliding 60s window check per bucket (bucket = e.g. "ip|general" or
        /// "ip|auth", so the tiers don't share a quota). Returns false when the
        /// caller is over <paramref name="limitPerMinute"/>;
        /// <paramref name="retryAfterSeconds"/> says how long until the oldest
        /// hit ages out.
        /// </summary>
        public bool CheckRateLimit(string bucketKey, int limitPerMinute, out int retryAfterSeconds)
        {
            retryAfterSeconds = 60;
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-1);
            var q = _hitsPerBucket.GetOrAdd(bucketKey, _ => new ConcurrentQueue<DateTime>());

            while (q.TryPeek(out var t) && t < cutoff)
                q.TryDequeue(out _);

            if (q.Count >= limitPerMinute)
            {
                if (q.TryPeek(out var oldest))
                {
                    var wait = (int)Math.Ceiling((oldest.AddMinutes(1) - now).TotalSeconds);
                    retryAfterSeconds = Math.Clamp(wait, 1, 60);
                }
                Interlocked.Increment(ref _rateLimited);
                return false;
            }

            q.Enqueue(now);
            return true;
        }

        private void PruneIdleBuckets()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-2);
            foreach (var kv in _hitsPerBucket)
            {
                var q = kv.Value;
                while (q.TryPeek(out var t) && t < cutoff)
                    q.TryDequeue(out _);
                if (q.IsEmpty)
                    _hitsPerBucket.TryRemove(kv.Key, out _);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cleanupTimer.Dispose();
        }
    }

    /// <summary>
    /// Load guard middleware (singleton IMiddleware so the counters are shared):
    /// <list type="bullet">
    /// <item>Concurrency cap — over <c>Load:MaxConcurrentRequests</c> in flight → 503 + Retry-After.</item>
    /// <item>Per-IP sliding-window rate limits — general API vs strict auth paths → 429 + Retry-After.</item>
    /// <item>Request body cap — Content-Length over <c>Load:MaxRequestBodyBytes</c> → 413.</item>
    /// <item>Slow-request logging — requests slower than <c>Load:SlowRequestThresholdSeconds</c> are logged.</item>
    /// </list>
    /// /api/health and /hub are exempt from rate limiting so monitoring and
    /// realtime connections stay reliable.
    /// </summary>
    public class LoadGuardMiddleware : IMiddleware
    {
        private readonly LoadOptions _opt;
        private readonly LoadStats _stats;
        private readonly ILogger<LoadGuardMiddleware> _log;

        // Substring hints (case-insensitive) for the strict auth rate limit.
        private static readonly string[] AuthPathHints =
            { "/register", "/login", "/connect/token", "exclusivelogin" };

        // Prefixes exempt from rate limiting (not from the concurrency gate).
        private static readonly string[] RateLimitExemptPrefixes =
            { "/api/health", "/hub" };

        public LoadGuardMiddleware(IOptions<LoadOptions> opt, LoadStats stats,
            ILogger<LoadGuardMiddleware> log)
        {
            _opt = opt.Value;
            _stats = stats;
            _log = log;
        }

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            if (!_opt.Enabled)
            {
                await next(ctx);
                return;
            }

            var path = ctx.Request.Path.Value ?? "/";

            // 1) Body cap — early 413 with a JSON body. Kestrel's own
            //    MaxRequestBodySize (set in Program.cs from the same setting)
            //    is the backstop for chunked requests without Content-Length.
            if (ctx.Request.ContentLength > _opt.MaxRequestBodyBytes)
            {
                ctx.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await ctx.Response.WriteAsJsonAsync(new
                {
                    error = $"request body too large (max {_opt.MaxRequestBodyBytes} bytes)"
                });
                return;
            }

            // 2) Concurrency gate — shed load instead of crashing.
            var concurrent = _stats.BeginRequest();
            try
            {
                if (concurrent > _opt.MaxConcurrentRequests)
                {
                    _stats.NoteOverloadReject();
                    _log.LogWarning(
                        "Overload: rejecting {Method} {Path} ({Concurrent} concurrent > max {Max})",
                        ctx.Request.Method, path, concurrent, _opt.MaxConcurrentRequests);
                    ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    ctx.Response.Headers.RetryAfter = "30";
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        error = "server busy, try again shortly"
                    });
                    return;
                }

                // 3) Per-IP sliding-window rate limit.
                if (!IsRateLimitExempt(path))
                {
                    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var isAuth = IsAuthPath(path);
                    var limit = isAuth ? _opt.AuthRequestsPerMinute : _opt.GeneralRequestsPerMinute;
                    // Separate buckets per tier so general traffic can't eat the auth quota.
                    var bucket = ip + (isAuth ? "|auth" : "|general");
                    if (!_stats.CheckRateLimit(bucket, limit, out var retryAfter))
                    {
                        _log.LogWarning("Rate limit: {Ip} exceeded {Limit}/min on {Path}",
                            ip, limit, path);
                        ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        ctx.Response.Headers.RetryAfter = retryAfter.ToString();
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "rate limit exceeded, slow down",
                            retryAfterSeconds = retryAfter
                        });
                        return;
                    }
                }

                // 4) Run the request, timing it for slow-request logging.
                var sw = Stopwatch.StartNew();
                try
                {
                    await next(ctx);
                }
                finally
                {
                    sw.Stop();
                    if (sw.Elapsed.TotalSeconds >= _opt.SlowRequestThresholdSeconds)
                    {
                        _log.LogWarning(
                            "Slow request: {Method} {Path} took {Ms:F0}ms (status {Status})",
                            ctx.Request.Method, path, sw.Elapsed.TotalMilliseconds,
                            ctx.Response.StatusCode);
                    }
                }
            }
            finally
            {
                _stats.EndRequest();
            }
        }

        private static bool IsAuthPath(string path)
        {
            foreach (var hint in AuthPathHints)
                if (path.Contains(hint, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static bool IsRateLimitExempt(string path)
        {
            foreach (var p in RateLimitExemptPrefixes)
                if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
