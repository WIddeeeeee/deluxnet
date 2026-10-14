using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class roomkeysController : ControllerBase
    {
        [HttpGet("v1/mine")]
        public IActionResult iojgwig()
        {
            return Ok(new List<object>());
        }
    }
}
