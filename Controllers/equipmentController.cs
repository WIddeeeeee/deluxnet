using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class equipmentController : ControllerBase
    {
        [HttpGet("v2/getUnlocked")]
        public IActionResult giwejigwioeg()
        {
            return Ok(new List<object>());
        }
    }
}
