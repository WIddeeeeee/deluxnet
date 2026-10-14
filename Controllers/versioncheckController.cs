using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class versioncheckController : ControllerBase
    {
        [HttpGet("v4")]
        public IActionResult checkVersion([FromQuery] string v)
        {
            return Ok(new
            {
                VersionStatus = 0
            });
        }
    }
}
