using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class messagesController : ControllerBase
    {
        [HttpGet("v2/get")]
        public IActionResult getmessagesjieiogio()
        {
            return Ok(new List<object>());
        }
    }
}
