using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class inventionsController : ControllerBase
    {
        [HttpGet("v2/mine")]
        public IActionResult getMyInvetions()
        {
            return Ok(new List<object>());
        }
    }
}
