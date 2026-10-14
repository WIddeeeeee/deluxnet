using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class announcementController : ControllerBase
    {
        [HttpGet("v1/get")]
        public IActionResult v1g()
        {
            return Ok(new List<object>());
        }

    }
}
