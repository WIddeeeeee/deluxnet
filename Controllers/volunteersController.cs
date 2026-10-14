using DeluxeNET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    /// <summary>
    /// Community volunteer server registry ("server browser").
    /// Anyone can volunteer their machine as a public game server:
    /// register once, heartbeat every ~60s to stay listed, and players
    /// pick from GET /api/volunteers.
    ///
    /// Config (appsettings "Volunteers" section):
    ///   Enabled                 = true   (master switch)
    ///   HeartbeatTimeoutSeconds = 180    (miss this long -> delisted)
    ///   MaxServers              = 200    (cap on active listings)
    ///   AllowPrivateHosts       = false  (true = allow 127.x/10.x/192.168.x
    ///                                     for LAN testing; never in prod)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class volunteersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public volunteersController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ---- rate limiting: 5 registrations/hour per client IP (in-memory) ----
        private static readonly ConcurrentDictionary<string, List<DateTime>> _regAttempts = new();
        private const int MaxRegsPerHour = 5;

        private bool RateLimited(string ip)
        {
            var now = DateTime.UtcNow;
            var list = _regAttempts.GetOrAdd(ip, _ => new List<DateTime>());
            lock (list)
            {
                list.RemoveAll(t => (now - t).TotalHours >= 1);
                if (list.Count >= MaxRegsPerHour) return true;
                list.Add(now);
                return false;
            }
        }

        private string ClientIp() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // ---- config helpers ----
        private bool VolunteersEnabled() => _config.GetValue("Volunteers:Enabled", true);
        private int HeartbeatTimeout() => _config.GetValue("Volunteers:HeartbeatTimeoutSeconds", 180);
        private int MaxServers() => _config.GetValue("Volunteers:MaxServers", 200);
        private bool AllowPrivateHosts() => _config.GetValue("Volunteers:AllowPrivateHosts", false);

        private DateTime LiveCutoff() => DateTime.UtcNow.AddSeconds(-HeartbeatTimeout());

        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string NewToken()
        {
            // 256-bit token, base64url — shown once at registration.
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        // ---- host validation ----
        private static readonly Regex HostnameRegex = new(
            @"^(?=.{1,253}$)(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.(?!-)[A-Za-z0-9-]{1,63}(?<!-))*$",
            RegexOptions.Compiled);

        private static bool IsPrivateOrLoopback(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return true;
            var b = ip.GetAddressBytes();
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return b[0] == 10
                    || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                    || (b[0] == 192 && b[1] == 168)
                    || (b[0] == 169 && b[1] == 254)   // link-local
                    || b[0] == 0;                      // 0.0.0.0/8
            }
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal;
        }

        /// <returns>null when valid, otherwise an error message.</returns>
        private string? ValidateHost(string host)
        {
            host = (host ?? "").Trim();
            if (host.Length == 0 || host.Length > 253)
                return "host is required (max 253 chars)";

            var lowered = host.ToLowerInvariant();
            if (lowered is "localhost" or "localhost.localdomain")
                return AllowPrivateHosts() ? null : "host must be a public address";

            if (IPAddress.TryParse(host, out var ip))
            {
                if (IsPrivateOrLoopback(ip) && !AllowPrivateHosts())
                    return "host must be a public IP or hostname";
                return null;
            }

            if (!HostnameRegex.IsMatch(host))
                return "host is not a valid IP or hostname";
            return null;
        }

        // ============================ endpoints ============================

        public class RegisterRequest
        {
            public string? Name { get; set; }
            public string? Host { get; set; }
            public int Port { get; set; }
            public int MaxPlayers { get; set; }
            public string? Description { get; set; }
            public string? Contact { get; set; }
        }

        /// <summary>Volunteer announces their server. Returns serverId + one-time token.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!VolunteersEnabled())
                return StatusCode(503, new { error = "volunteer hosting is disabled" });

            if (req == null) return BadRequest(new { error = "request body required" });

            if (RateLimited(ClientIp()))
                return StatusCode(429, new { error = "too many registrations, try again later" });

            var name = (req.Name ?? "").Trim();
            if (name.Length < 3 || name.Length > 60)
                return BadRequest(new { error = "name must be 3-60 characters" });

            var hostError = ValidateHost(req.Host ?? "");
            if (hostError != null) return BadRequest(new { error = hostError });

            if (req.Port is < 1 or > 65535)
                return BadRequest(new { error = "port must be 1-65535" });
            if (req.MaxPlayers is < 1 or > 10000)
                return BadRequest(new { error = "maxPlayers must be 1-10000" });

            var description = (req.Description ?? "").Trim();
            if (description.Length > 500)
                return BadRequest(new { error = "description max 500 characters" });
            var contact = (req.Contact ?? "").Trim();
            if (contact.Length > 120)
                return BadRequest(new { error = "contact max 120 characters" });

            var activeCount = await _db.VolunteerServers
                .CountAsync(v => v.IsActive && v.LastHeartbeat >= LiveCutoff());
            if (activeCount >= MaxServers())
                return StatusCode(503, new { error = "server list is full, try again later" });

            var token = NewToken();
            var server = new VolunteerServer
            {
                Name = name,
                Host = req.Host!.Trim(),
                Port = req.Port,
                MaxPlayers = req.MaxPlayers,
                CurrentPlayers = 0,
                Description = description.Length > 0 ? description : null,
                Contact = contact.Length > 0 ? contact : null,
                TokenHash = Sha256Hex(token),
                LastHeartbeat = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.VolunteerServers.Add(server);
            await _db.SaveChangesAsync();

            // Token is returned ONCE — only the hash is stored.
            return Ok(new { serverId = server.Id, token });
        }

        public class HeartbeatRequest
        {
            public long ServerId { get; set; }
            public string? Token { get; set; }
            public int CurrentPlayers { get; set; }
            public string? Status { get; set; }
        }

        /// <summary>Keep a listing alive. Send every ~60s; missing heartbeats
        /// for HeartbeatTimeoutSeconds delists the server.</summary>
        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest req)
        {
            if (!VolunteersEnabled())
                return StatusCode(503, new { error = "volunteer hosting is disabled" });

            if (req == null || string.IsNullOrEmpty(req.Token))
                return BadRequest(new { error = "serverId and token required" });

            var server = await _db.VolunteerServers.FindAsync(req.ServerId);
            if (server == null || !server.IsActive)
                return NotFound(new { error = "server not found or delisted" });

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(server.TokenHash),
                    Encoding.UTF8.GetBytes(Sha256Hex(req.Token))))
                return Unauthorized(new { error = "bad token" });

            server.CurrentPlayers = Math.Clamp(req.CurrentPlayers, 0, server.MaxPlayers);
            server.LastHeartbeat = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var ttl = HeartbeatTimeout();
            return Ok(new { ok = true, nextHeartbeatInSeconds = Math.Min(60, ttl / 3) });
        }

        /// <summary>PUBLIC list of live volunteer servers (server browser).</summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            if (!VolunteersEnabled())
                return Ok(new List<object>());

            var cutoff = LiveCutoff();
            var servers = await _db.VolunteerServers
                .Where(v => v.IsActive && v.LastHeartbeat >= cutoff)
                .OrderByDescending(v => v.CurrentPlayers)
                .Take(MaxServers())
                .Select(v => new
                {
                    id = v.Id,
                    name = v.Name,
                    host = v.Host,
                    port = v.Port,
                    currentPlayers = v.CurrentPlayers,
                    maxPlayers = v.MaxPlayers,
                    description = v.Description,
                    lastHeartbeat = v.LastHeartbeat
                    // NOTE: contact + token hash are NEVER exposed publicly.
                })
                .ToListAsync();

            return Ok(servers);
        }

        public class DeleteRequest
        {
            public string? Token { get; set; }
        }

        /// <summary>Volunteer delists their own server. Token required.</summary>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, [FromBody] DeleteRequest? req)
        {
            if (req == null || string.IsNullOrEmpty(req.Token))
                return BadRequest(new { error = "token required" });

            var server = await _db.VolunteerServers.FindAsync(id);
            if (server == null || !server.IsActive)
                return NotFound(new { error = "server not found or already delisted" });

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(server.TokenHash),
                    Encoding.UTF8.GetBytes(Sha256Hex(req.Token))))
                return Unauthorized(new { error = "bad token" });

            server.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, delisted = id });
        }
    }
}
