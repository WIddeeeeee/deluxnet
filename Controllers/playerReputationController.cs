using DeluxeNET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class playerReputationController : ControllerBase
    {
        private readonly AppDbContext _db;
        public playerReputationController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("v2/bulk")]
        public async Task<IActionResult> getRepBulk([FromQuery] List<long> id)
        {
            // FIX: cap the batch so one request can't dump the whole table.
            var reps = await _db.Reputations
                .Where(x => id.Contains(x.Id))
                .Take(100)
                .ToListAsync();

            return Ok(reps);
        }
    }
}
