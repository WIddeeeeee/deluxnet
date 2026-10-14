using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusCardController : ControllerBase
    {
        private readonly jwt _jwt;

        public CampusCardController(jwt __jwt)
        {
            _jwt = __jwt;
        }


        [HttpPost("v1/UpdateAndGetSubscription")]
        public IActionResult getMySub()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"]);

            if (accountId == null) return Unauthorized();

            return Ok(new
            {
                CanBuySubscription = true,
                PlatformAccountSubscribedPlayerId = accountId,
                Subscription = new
                {
                    CreatedAt = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddDays(67),
                    IsActive = true,
                    IsAutoRenewing = true,
                    Level = 0,
                    ModifiedAt = DateTime.UtcNow.AddHours(1),
                    Period = 0,
                    PlatformId = "67",
                    PlatformPurchaseId = "0",
                    PlatformType = 0,
                    RecNetPlayerId = accountId,
                    SubscriptionId = 0
                }
            });
        }
    }
}
