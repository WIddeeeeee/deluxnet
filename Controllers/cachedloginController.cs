using DeluxeNET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class cachedloginController : ControllerBase
    {
        private readonly AppDbContext _db;

        public cachedloginController(AppDbContext db)
        {
            _db = db;
        }


        [HttpGet("forplatformid/{platform}/{platformid}")]
        public async Task<IActionResult> GetCachedLogins([FromRoute] int platform, [FromRoute] string platformid)
        {
            var cachedlogins = await _db.CachedLogins
                .Where(x => x.platformId == platformid && x.platform == platform)
                .ToListAsync();

            return Ok(cachedlogins);
        }
    }
}
