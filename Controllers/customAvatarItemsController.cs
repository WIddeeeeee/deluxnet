using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class customAvatarItemsController : ControllerBase
    {
        [HttpGet("v1/isCreationAllowedForAccount")]
        public IActionResult caf()
        {
            return Ok(new
            {
                success = true,
                value = ""
            });
        }

        [HttpGet("v1/isRenderingEnabled")]
        public IActionResult ire()
        {
            return Ok(true);
        }

        [HttpGet("v1/isCreationEnabled")]
        public IActionResult ice()
        {
            return Ok(true);
        }

        [HttpGet("v2/fromcreator/{playerid}")]
        public IActionResult getCustomAvatarItemsFromCreator()
        {
            return Ok(new
            {
                Results = new List<object>(),
                TotalResults = 0
            });
        }
    }
}
