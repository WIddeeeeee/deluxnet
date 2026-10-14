using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerReportingController : ControllerBase
    {
        [HttpGet("v1/moderationBlockDetails")]
        public IActionResult modblockdetails()
        {
            return Ok(new
            {
                ReportCategory = 0,
                Duration = 0,
                GameSessionId = 0,
                Message = ""
            });
        }

        [HttpPost("v1/hile")]
        public IActionResult hile()
        {
            return Ok(false);
        }

        [HttpGet("v1/voteToKickReasons")]
        public IActionResult vtkr()
        {
            return Ok(new List<object>());
        }
    }
}
