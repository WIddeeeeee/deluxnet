using DeluxeNET.Data;
using DeluxeNET.Jsons;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class avatarController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        public avatarController(AppDbContext db, jwt jwt)
        {
            _db = db;
            _jwt = jwt;
        }


        [HttpGet("v1/defaultunlocked")]
        public IActionResult getDefaultUnlockedAvatarITems()
        {
            return Ok(new List<object>());
        }

        [HttpGet("v1/defaultbaseavataritems")]
        public IActionResult getDefaultBaseItems()
        {
            return Ok(new List<object>());
        }

        [HttpGet("v4/items")]
        public IActionResult getUnlockedAvatarItems()
        {
            // FIX: was a 500 if clothing.json was missing.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Jsons", "clothing.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<object>());

            var clothingfile = System.IO.File.ReadAllText(path);

            var clothing = JsonConvert.DeserializeObject<List<avatarItem>>(clothingfile);

            return Ok(clothing);
        }

        [HttpGet("v2")]
        public async Task<IActionResult> getMyAvatar()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"]);

            if (accountId == null) return Unauthorized();

            var avatardetails = await _db.Avatars
                .FirstOrDefaultAsync(x => x.accountId == accountId);

            if (avatardetails == null)
            {
                // FIX: was returning a fabricated object with a hardcoded Id=67
                // instead of the row just inserted. Re-query the real row.
                _db.Avatars.Add(new avatar
                {
                    accountId = accountId
                });

                await _db.SaveChangesAsync();

                avatardetails = await _db.Avatars
                    .FirstOrDefaultAsync(x => x.accountId == accountId);
            }

            return Ok(avatardetails);
        }

        [HttpGet("v3/saved")]
        public IActionResult getsavedoutfits()
        {
            return Ok(new List<object>());
        }

        [HttpGet("v2/gifts")]
        public IActionResult getGifts()
        {
            return Ok(new List<object>());
        }

        public class SetAvatarRequest
        {
            public string? OutfitSelections { get; set; }
            public string? FaceFeatures { get; set; }
            public string? SkinColor { get; set; }
            public string? HairColor { get; set; }
        }

        [HttpPost("v2/set")]
        public async Task<IActionResult> setAvatar([FromBody] SetAvatarRequest body)
        {
            // FIX: was a no-op stub. Now actually stores the avatar.
            // (Binds a DTO, not the entity — the entity's required accountId
            // would 400 every client call.)
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"]);
            if (accountId == null) return Unauthorized();

            var av = await _db.Avatars.FirstOrDefaultAsync(x => x.accountId == accountId);
            if (av == null)
            {
                av = new avatar { accountId = accountId };
                _db.Avatars.Add(av);
            }

            av.OutfitSelections = body.OutfitSelections ?? av.OutfitSelections;
            av.FaceFeatures = body.FaceFeatures ?? av.FaceFeatures;
            av.SkinColor = body.SkinColor ?? av.SkinColor;
            av.HairColor = body.HairColor ?? av.HairColor;
            await _db.SaveChangesAsync();

            return Ok(av);
        }
    }

    
}
