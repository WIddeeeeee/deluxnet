using DeluxeNET.Data;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class accountController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        public accountController(AppDbContext db, jwt __jwt)
        {
            _db = db;
            _jwt = __jwt;
        }

        [HttpGet("bulk")]
        public async Task<IActionResult> BulkAccountDetails([FromQuery] List<long> id)
        {
            // FIX: cap the batch so one request can't dump the whole table.
            var accounts = await _db.Accounts
                .Where(x => id.Contains(x.accountId))
                .Take(100)
                .ToListAsync();

            return Ok(accounts);
        }

        // Public player search for the local web UI.
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 2)
                return BadRequest(new { error = "search needs at least 2 characters" });

            var q = username.Trim();
            var matches = await _db.Accounts
                .Where(x => x.username.Contains(q) || x.displayName.Contains(q))
                .Take(20)
                .Select(x => new
                {
                    x.accountId,
                    x.username,
                    x.displayName,
                    x.profileImage,
                    x.isJunior
                })
                .ToListAsync();

            return Ok(matches);
        }

        [HttpGet("me")]
        public async Task<IActionResult> getMyAccountDetails()
        {
            // FIX: don't dump request headers — the Authorization header is a live token.
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            if (accountId == null) return Unauthorized();

            var userdetails = await _db.Accounts
                .FirstOrDefaultAsync(x => x.accountId == accountId);

            if (userdetails == null) return NotFound();

            return Ok(userdetails);
        }

        [HttpGet("{playerid}/bio")]
        public async Task<IActionResult> getPlayerBio([FromRoute] long playerid)
        {
            var player = await _db.Accounts
                .FirstOrDefaultAsync(x => x.accountId == playerid);

            // FIX: was a NullReferenceException (500) for unknown players.
            if (player == null) return NotFound();

            return Ok(new
            {
                accountId = playerid,
                bio = player.bio
            });
        }

        public class RegisterRequest
        {
            public required string username { get; set; }
            public string? displayName { get; set; }
        }

        // Creates a new account. Accounts are NOT junior by default — junior is
        // only set when another player reports the account as under 13.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var username = req.username?.Trim() ?? "";
            if (username.Length < 3 || username.Length > 24)
                return BadRequest(new { error = "username must be 3-24 characters" });

            var exists = await _db.Accounts.AnyAsync(x => x.username == username);
            if (exists) return Conflict(new { error = "username is taken" });

            var account = new account
            {
                username = username,
                displayName = (req.displayName ?? username).Trim(),
                isJunior = false,
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(account.accountId.ToString());

            return Ok(new
            {
                account,
                access_token = token
            });
        }

    }
}
