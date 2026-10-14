using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class objectivesController : ControllerBase
    {
        [HttpGet("v1/myprogress")]
        public IActionResult getMyProgress()
        {
            return Ok(new
            {
                Objectives = new List<object>(),
                ObjectiveGroups = new List<object>()
            });
        }
    }
}
