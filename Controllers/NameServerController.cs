using DeluxeNET.Models.ServerConfiguration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("/")]
    [ApiController]
    public class NameServerController : ControllerBase
    {
        private readonly string BaseURL;
        private readonly string[] WebHostnames;

        public NameServerController(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            // FIX: was hardcoded to https://localhost — now set Public:BaseUrl
            // in appsettings.json to your public address.
            BaseURL = (config["Public:BaseUrl"] ?? "https://localhost").TrimEnd('/');
            // Hostnames that should serve the community website instead of the
            // name-server JSON (e.g. players who pointed rec.net at this server
            // via their hosts file). Game clients connect by IP, so they always
            // get the JSON.
            WebHostnames = config.GetSection("Web:Hostnames").Get<string[]>() ?? new[] { "rec.net" };
        }

        [HttpGet]
        public ActionResult NameServer()
        {
            var host = Request.Host.Host.ToLowerInvariant();
            if (WebHostnames.Any(d =>
            {
                d = d.Trim().ToLowerInvariant();
                return host.Length > 0 && (host == d || host.EndsWith("." + d));
            }))
                return Redirect("/web/index.html");

            return Ok(new NameServerModel
            {
                Accounts = BaseURL,
                API = BaseURL,
                Auth = BaseURL,
                BugReporting = BaseURL,
                Cards = BaseURL,
                CDN = BaseURL+"/cdnserver",
                Chat = BaseURL,
                Clubs = BaseURL,
                CMS = BaseURL,
                Commerce = BaseURL,
                Data = BaseURL,
                DataCollection = BaseURL,
                Discovery = BaseURL,
                Econ = BaseURL,
                GameLogs = BaseURL,
                Geo = BaseURL,
                Images = BaseURL+"/imageserver",
                Leaderboard = BaseURL,
                Link = BaseURL,
                Lists = BaseURL,
                Matchmaking = BaseURL,
                Moderation = BaseURL,
                Notifications = BaseURL,
                PlatformNotifications = BaseURL,
                PlayerSettings = BaseURL,
                RoomComments = BaseURL,
                Rooms = BaseURL+"/roomserver",
                Storage = BaseURL+"/cdnserver",
                Strings = BaseURL,
                StringsCDN = BaseURL+"/cdnserver",
                Studio = BaseURL,
                Thorn = BaseURL,
                Videos = BaseURL,
                WWW = BaseURL
            });
        }
    }
}
