using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class communityboardController : ControllerBase
    {
        [HttpGet("v2/current")]
        public async Task<IActionResult> getcb()
        {
            // FIX: was a 500 if communityboard.json was missing.
            var p = Path.Combine(Directory.GetCurrentDirectory(), "Jsons", "communityboard.json");
            if (!System.IO.File.Exists(p)) return Ok(new List<object>());

            var j = JsonConvert.DeserializeObject(await System.IO.File.ReadAllTextAsync(p));

            return Ok(j);
        }
    }
}
