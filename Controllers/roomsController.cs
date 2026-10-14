using DeluxeNET.Data;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class roomsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        public roomsController(AppDbContext db, jwt jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpGet("v1/filters")]
        public IActionResult GetFilters()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());
            if (accountId == null) return Unauthorized();

            return Ok(new
            {
                PinnedFilers = new List<string>
                {
                    "recroomoriginal","community","quest","puzzle","pvp","hangout","art","tutorial",
                    "fandom","performance","action","horror"
                },
                PopularFilters = new List<string>
                {
                    "recroomoriginal","quest","community","import"
                }
            });
        }


        //[HttpGet("requiring/developer")]
        //public IActionResult getDoesRequireDev()
        //{
        //    return Ok(false);
        //}

    }
}
