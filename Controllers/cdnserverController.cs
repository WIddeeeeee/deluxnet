using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class cdnserverController : ControllerBase
    {
        [HttpGet("config/LoadingScreenTipData")]
        public IActionResult getthestupideitiowetiewj()
        {
            return Ok(new List<object>());
        }
    }
}
