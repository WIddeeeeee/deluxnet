using DeluxeNET.Data;
using DeluxeNET.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace DeluxeNET.Controllers
{
    // Load/health probe for operators and load balancers.
    // Exempt from rate limiting (see LoadGuardMiddleware) so monitoring stays
    // reliable even when the API is throttled.
    [Route("api/[controller]")]
    [ApiController]
    public class healthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly LoadStats _stats;
        private readonly LoadOptions _opt;

        public healthController(AppDbContext db, LoadStats stats, IOptions<LoadOptions> opt)
        {
            _db = db;
            _stats = stats;
            _opt = opt.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // DB latency probe.
            double dbMs;
            bool dbOk;
            var sw = Stopwatch.StartNew();
            try
            {
                await _db.Database.ExecuteSqlRawAsync("SELECT 1");
                sw.Stop();
                dbMs = sw.Elapsed.TotalMilliseconds;
                dbOk = true;
            }
            catch
            {
                sw.Stop();
                dbMs = -1;
                dbOk = false;
            }

            long online = 0;
            if (dbOk)
            {
                try
                {
                    var cutoff = DateTime.UtcNow.AddMinutes(-5);
                    online = await _db.Heartbeats
                        .CountAsync(h => h.IsOnline && h.LastOnline >= cutoff);
                }
                catch
                {
                    dbOk = false;
                }
            }

            var concurrent = _stats.Concurrent;
            var status = "ok";
            if (!dbOk)
                status = "unhealthy";
            else if (concurrent > _opt.MaxConcurrentRequests * 0.8 || dbMs > 2000)
                status = "degraded";

            return Ok(new
            {
                status,
                uptimeSeconds = (long)(DateTime.UtcNow - _stats.StartedAt).TotalSeconds,
                concurrentRequests = concurrent,
                maxConcurrentRequests = _opt.MaxConcurrentRequests,
                requestsPerSecond = Math.Round(_stats.RequestsPerSecond(), 2),
                totalRequests = _stats.TotalRequests,
                rateLimitedRequests = _stats.RateLimited,
                overloadRejections = _stats.OverloadRejections,
                dbLatencyMs = Math.Round(dbMs, 1),
                playersOnline = online,
                limits = new
                {
                    generalRequestsPerMinute = _opt.GeneralRequestsPerMinute,
                    authRequestsPerMinute = _opt.AuthRequestsPerMinute,
                    maxRequestBodyBytes = _opt.MaxRequestBodyBytes
                }
            });
        }
    }
}
