using DeluxeNET.Data;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeluxeNET.Controllers
{
    [Route("/")]
    [ApiController]
    public class playersettingsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        public playersettingsController(AppDbContext db, jwt jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpGet("playersettings")]
        public async Task<IActionResult> getSettings()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            if (accountId == null) return Unauthorized();

            var settings = await _db.Settings
                .Where(x => x.accountId == accountId)
                .ToListAsync();

            settings.Add(new setting { accountId = accountId, Key = "TUTORIAL_COMPLETED_MASK", Value = "123"});
            settings.Add(new setting { accountId = accountId, Key = "HAS_COMPLETED_ORIENTATION", Value = "True" });
            settings.Add(new setting { accountId = accountId, Key = "OrientationCompletionTime", Value = "2024-07-09T07:53:30.7953332Z" });



            return Ok(settings);
        }



        [HttpGet("api/settings/v2/")]
        public async Task<IActionResult> getSettingsOLD()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            if (accountId == null) return Unauthorized();

            var settings = await _db.Settings
                .Where(x => x.accountId == accountId)
                .ToListAsync();

            settings.Add(new setting { accountId = accountId, Key = "TUTORIAL_COMPLETED_MASK", Value = "123" });
            settings.Add(new setting { accountId = accountId, Key = "HAS_COMPLETED_ORIENTATION", Value = "True" });
            settings.Add(new setting { accountId = accountId, Key = "OrientationCompletionTime", Value = "2024-07-09T07:53:30.7953332Z" });



            return Ok(settings);
        }

        [HttpPut("playersettings")]
        public async Task<IActionResult> setSetting()
        {
            var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            if (accountId == null) return Unauthorized();

            // FIX: was a 500 when the client didn't send form data.
            if (!Request.HasFormContentType)
                return BadRequest(new { error = "expected form data" });

            var key = Request.Form["Key"].ToString().Trim();
            var value = Request.Form["Value"].ToString();
            if (string.IsNullOrEmpty(key) || key.Length > 128)
                return BadRequest(new { error = "Key is required (max 128 chars)" });

            var setting = await _db.Settings
                .FirstOrDefaultAsync(x => x.Key == key && x.accountId == accountId);

            

            if (setting == null)
            {
                _db.Settings.Add(new setting
                {
                    accountId = accountId,
                    Key = key,
                    Value = value
                });
            } else
            {
                setting.Value = value;
            }

                await _db.SaveChangesAsync();

            return Ok(new List<object>());

        }


        [HttpPost("api/settings/v2/set")]
        public async Task<IActionResult> setSettingOLD()
        {
            //var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            //if (accountId == null) return Unauthorized();

            //var setting = await _db.Settings
            //    .FirstOrDefaultAsync(x => x.Key == Request.Form["Key"].ToString() && x.accountId == accountId);



            //if (setting == null)
            //{
            //    _db.Settings.Add(new setting
            //    {
            //        accountId = accountId,
            //        Key = Request.Form["Key"].ToString(),
            //        Value = Request.Form["Value"].ToString()
            //    });
            //}
            //else
            //{
            //    setting.Value = Request.Form["Value"].ToString();
            //}

            //await _db.SaveChangesAsync();

            return Ok(new List<object>());

        }
    }
}
