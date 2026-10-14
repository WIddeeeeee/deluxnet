using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gamerewardsController : ControllerBase
    {
        [HttpGet("v1/pending")]
        public IActionResult getpendingrewards()
        {
            return Ok(new List<object>());
        }
    }
}
