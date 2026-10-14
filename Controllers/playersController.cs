using DeluxeNET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class playersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public playersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("v2/progression/bulk")]
        public async Task<IActionResult> getprogressBulk([FromQuery] List<long> id)
        {
            // FIX: cap the batch so one request can't dump the whole table.
            var progressions = await _db.Progressions
                .Where(x => id.Contains(x.Id))
                .Take(100)
                .ToListAsync();

            return Ok(progressions);
        }
    }
}
