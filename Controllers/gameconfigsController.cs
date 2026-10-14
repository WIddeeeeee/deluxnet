using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeluxeNET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gameconfigsController : ControllerBase
    {
        [HttpGet("v1/amplitude")]
        public IActionResult getAmpl()
        {
            return Ok(new
            {
                AmplitudeKey = "7b69edb8ca6d2934989599a4ca9f7ca5"
            });
        }

        [HttpGet("v1/all")]
        public IActionResult getAllConf()
        {
            return Ok(new List<object>());
        }

        [HttpGet("v1/backtrace")]
        public IActionResult backtrace()
        {
            return Ok(new
            {
                ReportBudget = 0,
                FilterType = 0,
                SampleRate = 0.025f,
                LogLineCount = 50,
                CaptureNativeCrashes = 1,
                ANRThresholdMs = 0,
                MessageCount = 1000,
                MessageRegex = "^Cannot set the parent of the GameObject .* while its new parent|^\\\u003E\\x2010x\\:\\x20|\\'LabelTheme\\' contains missing PaletteTheme reference on",
                VersionRegex = ".*"
            });
        }

        [HttpGet("v2")]
        public async Task<IActionResult> getconfv2()
        {
            var p = Path.Combine(Directory.GetCurrentDirectory(), "Jsons", "configv2.json");

            // FIX: was Ok(string) which double-encodes the JSON, and 500'd when
            // the file was missing. Serve the raw JSON with the right content type.
            if (!System.IO.File.Exists(p)) return NotFound();

            var rawj = await System.IO.File.ReadAllTextAsync(p);

            return Content(rawj, "application/json");
        }
    }
}
