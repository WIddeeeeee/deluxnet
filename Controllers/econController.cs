using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class econController : ControllerBase
    {
        [HttpGet("customAvatarItems/v1/owned")]
        public IActionResult getMyOwnedCustomAvatarItems()
        {
            return Ok(new
            {
                Results = new List<object>(),
                TotalResults = 0
            });
        }
    }
}
