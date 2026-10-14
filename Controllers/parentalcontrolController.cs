using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class parentalcontrolController : ControllerBase
    {
        private readonly jwt _jwt;

        public parentalcontrolController(jwt jwt)
        {
            _jwt = jwt;
        }


        [HttpGet("me")]
        public IActionResult getMyParentalControlConf()
        {

            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            if (accountId == null) return Unauthorized();

            return Ok(new
            {
                accountId = accountId,
                disallowInAppPurchases = false
            });
        }
    }
}
