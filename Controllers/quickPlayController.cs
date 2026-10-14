using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class quickPlayController : ControllerBase
    {
        [HttpGet("v1/getandclear")]
        public IActionResult getandclear()
        {
            return Ok(new { });
        }
    }
}
