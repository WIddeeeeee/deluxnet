using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class consumablesController : ControllerBase
    {
        [HttpGet("v2/getUnlocked")]
        public IActionResult getUnlockedCons()
        {
            return Ok(new List<object>());
        }
    }
}
