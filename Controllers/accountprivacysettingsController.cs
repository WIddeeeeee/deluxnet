using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class accountprivacysettingsController : ControllerBase
    {
        [HttpGet("{playerid}")]
        public IActionResult getAccountPrivacy([FromRoute] long playerid)
        {
            return Ok(new
            {
                accountId = playerid,
                isRecentHistoryVisible = true
            });
        }
    }
}
