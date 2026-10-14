using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class progressionEventsController : ControllerBase
    {
        [HttpGet("active")]
        public IActionResult getActiveProgressionEvents()
        {
            return NoContent();
        }
    }
}
