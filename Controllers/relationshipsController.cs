using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class relationshipsController : ControllerBase
    {
        [HttpGet("v2/get")]
        public IActionResult getReplationshgnowroighwrohgo()
        {
            return Ok(new List<object>());
        }
    }
}
