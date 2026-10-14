using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class playereventsController : ControllerBase
    {
        [HttpGet("v1/all")]
        public IActionResult getCurrentEvents()
        {
            return Ok(new
            {
                Created = new List<object>(),
                Responses = new List<object>()
            });
        }
    }
}
