using DeluxeNET.Data;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class dataController : ControllerBase
    {
        private readonly jwt _jwt;
        private readonly AppDbContext _db;

        public dataController(jwt __jwt, AppDbContext __db)
        {
            _jwt = __jwt;
            _db = __db;
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> getmypresnece()
        {
            //var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());
            //if (accountId == null) return Unauthorized();

            //var heartbeatdata = await _db.Heartbeats
            //    .FirstOrDefaultAsync(x => x.PlayerId == accountId);

            //if (heartbeatdata == null) return NotFound();

            return Ok(new
            {
                Success = true
            });
        }
    }
}
