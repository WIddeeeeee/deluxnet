using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class subscriptionController : ControllerBase
    {
        [HttpGet("subscriberCount/{playerid}")]
        public IActionResult subscribercpuntget()
        {
            return Ok(0);
        }
    }
}
