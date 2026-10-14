using DeluxeNET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    // Public server stats for the local web UI (and anyone else).
    [Route("api/[controller]")]
    [ApiController]
    public class statsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public statsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            return Ok(new
            {
                players = await _db.Accounts.CountAsync(),
                rooms = await _db.Rooms.CountAsync(),
                online = await _db.Heartbeats.CountAsync(h => h.IsOnline && h.LastOnline >= cutoff),
                visits = await _db.RoomStats.SumAsync(s => (long?)s.VisitCount) ?? 0
            });
        }
    }
}
