using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class checklistController : ControllerBase
    {
        [HttpGet("v1/current")]
        public IActionResult getMyStfehfowoji()
        {
            return Ok(new List<object>());
        }
    }
}
