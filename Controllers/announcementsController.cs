using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class announcementsController : ControllerBase
    {
        [HttpGet("v2/mine/unread")]
        [HttpGet("v2/subscription/mine/unread")]
        public IActionResult fjweiio()
        {
            return Ok(new List<object>());
        }
    }
}
