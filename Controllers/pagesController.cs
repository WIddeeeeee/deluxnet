using DeluxeNET.Data;
using DeluxeNET.Security;
using DeluxeNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace DeluxeNET.Controllers
{
    /// <summary>
    /// Player webpage hosting on rec.net (GeoCities-style). Authenticated
    /// players can publish a personal HTML page; anyone can view it.
    /// User HTML is sanitized at render time (see HtmlSanitizer).
    /// </summary>
    [ApiController]
    public class pagesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        // Rate limiting: max page updates per account per hour (in-memory).
        private static readonly ConcurrentDictionary<long, Queue<DateTime>> _updateLog = new();
        private const int MaxUpdatesPerHour = 10;
        private const int MaxHtmlBytes = 51200; // 50KB

        public pagesController(AppDbContext db, jwt __jwt)
        {
            _db = db;
            _jwt = __jwt;
        }

        private long? AuthAccountId()
        {
            return _jwt.VerifyToken(Request.Headers["Authorization"].ToString());
        }

        // ---------- Authenticated API ----------

        [HttpPut("api/pages/me")]
        public async Task<IActionResult> SetMyPage([FromBody] SetPageRequest req)
        {
            var accountId = AuthAccountId();
            if (accountId == null) return Unauthorized();

            if (!CheckRateLimit(accountId.Value))
                return StatusCode(429, new { error = "rate limited: max 10 page updates per hour" });

            var html = req?.Html ?? "";
            if (Encoding.UTF8.GetByteCount(html) > MaxHtmlBytes)
                return BadRequest(new { error = "page too large: 50KB max" });

            var page = await _db.PlayerPages
                .FirstOrDefaultAsync(p => p.AccountId == accountId.Value);

            if (page == null)
            {
                page = new PlayerPage { AccountId = accountId.Value, HtmlContent = html };
                _db.PlayerPages.Add(page);
            }
            else
            {
                page.HtmlContent = html;
                page.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            var username = (await _db.Accounts.FindAsync(accountId.Value))?.username ?? "?";
            return Ok(new { url = $"/users/{username}", updatedAt = page.UpdatedAt });
        }

        [HttpGet("api/pages/me")]
        public async Task<IActionResult> GetMyPage()
        {
            var accountId = AuthAccountId();
            if (accountId == null) return Unauthorized();

            var page = await _db.PlayerPages
                .FirstOrDefaultAsync(p => p.AccountId == accountId.Value);
            if (page == null) return NotFound(new { error = "no page yet" });

            // Return the RAW html for editing (sanitization happens at render).
            return Ok(new { html = page.HtmlContent, updatedAt = page.UpdatedAt });
        }

        [HttpDelete("api/pages/me")]
        public async Task<IActionResult> DeleteMyPage()
        {
            var accountId = AuthAccountId();
            if (accountId == null) return Unauthorized();

            var page = await _db.PlayerPages
                .FirstOrDefaultAsync(p => p.AccountId == accountId.Value);
            if (page == null) return NotFound(new { error = "no page yet" });

            _db.PlayerPages.Remove(page);
            await _db.SaveChangesAsync();
            return Ok(new { deleted = true });
        }

        // ---------- Public views ----------

        /// <summary>JSON directory of players who have pages.</summary>
        [HttpGet("api/pages/directory")]
        [HttpGet("/users")]
        public async Task<IActionResult> Directory()
        {
            var list = await _db.PlayerPages
                .Join(_db.Accounts,
                    p => p.AccountId,
                    a => a.accountId,
                    (p, a) => new { a.username, a.displayName, p.UpdatedAt })
                .OrderByDescending(x => x.UpdatedAt)
                .Take(200)
                .ToListAsync();
            return Ok(list);
        }

        /// <summary>Public rendered page: /users/{username}</summary>
        [HttpGet("/users/{username}")]
        public async Task<IActionResult> ViewByName(string username)
        {
            return await RenderPage(username);
        }

        // NOTE: /~{username} is not possible — ASP.NET Core forbids route
        // templates starting with '~' unless followed by '/'. /users/{username}
        // is the canonical public URL (page still brands itself rec.net/~name).

        private async Task<IActionResult> RenderPage(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return NotFound();

            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.username == username.Trim());
            if (account == null) return NotFound("No such player.");

            var page = await _db.PlayerPages
                .FirstOrDefaultAsync(p => p.AccountId == account.accountId);
            if (page == null)
                return Content(
                    WrapPage(username, "<p><i>This player hasn't made a page yet.</i></p>", null),
                    "text/html");

            var safe = HtmlSanitizer.Sanitize(page.HtmlContent);
            return Content(WrapPage(username, safe, page.UpdatedAt), "text/html");
        }

        private static bool CheckRateLimit(long accountId)
        {
            var q = _updateLog.GetOrAdd(accountId, _ => new Queue<DateTime>());
            lock (q)
            {
                var cutoff = DateTime.UtcNow.AddHours(-1);
                while (q.Count > 0 && q.Peek() < cutoff) q.Dequeue();
                if (q.Count >= MaxUpdatesPerHour) return false;
                q.Enqueue(DateTime.UtcNow);
                return true;
            }
        }

        /// <summary>DeluxNet-branded wrapper rendered around sanitized user HTML.</summary>
        private static string WrapPage(string username, string bodyHtml, DateTime? updatedAt)
        {
            var safeName = WebUtility.HtmlEncode(username);
            var updated = updatedAt.HasValue
                ? $"<span class=\"updated\">updated {updatedAt.Value:yyyy-MM-dd}</span>" : "";
            return $@"<!doctype html>
<html><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>rec.net/~{safeName}</title>
<style>
body{{background:#14100c;color:#f5f0e8;font-family:system-ui,sans-serif;margin:0}}
.hdr{{background:#1e1712;border-bottom:3px solid #ff7a1a;padding:12px 20px;display:flex;align-items:center;gap:14px}}
.hdr .brand{{color:#ff7a1a;font-weight:800;font-size:20px;text-decoration:none}}
.hdr .path{{color:#cbbfae;font-size:15px}}
.hdr .spacer{{flex:1}}
.hdr a.home{{color:#ff7a1a;text-decoration:none;font-size:14px}}
.updated{{color:#8a7f70;font-size:12px}}
.page{{max-width:800px;margin:24px auto;padding:0 20px 60px;line-height:1.6}}
.page a{{color:#ff9a3c}}
.page img{{max-width:100%}}
.ftr{{text-align:center;color:#8a7f70;font-size:12px;padding:24px;border-top:1px solid #2a211a}}
.ftr a{{color:#ff7a1a}}
</style></head><body>
<div class=""hdr""><a class=""brand"" href=""/web/index.html"">NEXTNOX</a>
<span class=""path"">rec.net/~{safeName}</span>{updated}
<span class=""spacer""></span><a class=""home"" href=""/web/index.html"">← main site</a></div>
<div class=""page"">{bodyHtml}</div>
<div class=""ftr"">Personal page of <b>{safeName}</b> · hosted on <a href=""/web/index.html"">rec.net</a> · content is the player's own</div>
</body></html>";
        }

        public class SetPageRequest
        {
            public string? Html { get; set; }
        }
    }
}
