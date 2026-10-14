using DeluxeNET.Data;
using DeluxeNET.Hubs;
using DeluxeNET.Security;
using DeluxeNET.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeluxeNET
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // First-run setup: a double-clicked exe with no database yet walks
            // through installing MariaDB and creating everything (secrets go to
            // Windows Credential Manager). Normal runs skip this silently.
            await FirstRunSetup.EnsureReadyAsync(builder.Configuration);

            // Add services to the container.

            // FIX: EF navigation properties form cycles (room -> subrooms -> room).
            // Ignore cycles globally instead of 500ing on serialization.
            builder.Services.AddControllers().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            var connectionString = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            });

            builder.Services.AddSingleton<jwt>();
            // FIX: removed AddSingleton<NotificationHub>() — hubs must not be
            // singletons (their Context/Clients are per-connection). Controllers
            // broadcast via IHubContext<NotificationHub> instead.
            builder.Services.AddSignalR();

            // Load management: rate limiting, overload protection, health metrics.
            // Settings live under "Load:" in appsettings.json.
            builder.Services.Configure<LoadOptions>(builder.Configuration.GetSection("Load"));
            builder.Services.AddSingleton<LoadStats>();
            builder.Services.AddSingleton<LoadGuardMiddleware>();

            // Server-level request body cap (backstop for chunked uploads with
            // no Content-Length; the middleware returns a nicer 413 first).
            if (builder.Configuration.GetValue<bool>("Load:Enabled", true))
            {
                var maxBody = builder.Configuration.GetValue<long>(
                    "Load:MaxRequestBodyBytes", 10L * 1024 * 1024);
                builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = maxBody);
            }

            // Optional DNS server for private-server play (off by default).
            // Serves local records (e.g. rec.net -> this machine) and forwards
            // everything else upstream. Enable with Dns:Enabled=true.
            if (builder.Configuration.GetValue<bool>("Dns:Enabled"))
                builder.Services.AddHostedService<DnsService>();

            var app = builder.Build();

            // Auto-apply EF migrations on startup so a fresh/empty database just works.
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.MapHub<NotificationHub>("/hub/v1");

            // Load guard first: concurrency cap (503), per-IP rate limits (429),
            // request body cap (413), slow-request logging. /api/health and /hub
            // are exempt from rate limiting so monitoring and realtime stay up.
            app.UseMiddleware<LoadGuardMiddleware>();

            // Local rec.net-style website (static files under wwwroot/web).
            // The game name-server stays on GET / — the site lives at /web/.
            app.UseStaticFiles();

            app.Use(async (ctx, next) =>
            {
                await next();

                var status = ctx.Response.StatusCode;
                var ct = ctx.Request.ContentType?.ToLower() ?? "";

                var type = ctx.Request.Method == "POST"
                    ? (ct.Contains("application/json") ? "JSON"
                    : ct.Contains("application/x-www-form-urlencoded") || ct.Contains("multipart/form-data") ? "FORM"
                    : ct.StartsWith("image/") ? "IMAGE"
                    : "BODY")
                    : "N/A";
                var url = ctx.Request.Path + ctx.Request.QueryString.Value;

                try
                {
                    ConsoleColor color =
                        status >= 200 && status < 300 ? ConsoleColor.Green :
                        status >= 300 && status < 400 ? ConsoleColor.Yellow :
                        status >= 400 && status < 500 ? ConsoleColor.Red :
                        status >= 500 ? ConsoleColor.DarkRed :
                        ConsoleColor.Gray;

                    Console.ForegroundColor = color;
                    Console.WriteLine($"{status} {ctx.Request.Method} {url} [{type}]");
                    Console.ResetColor();
                }
                catch
                {
                    // No console attached (e.g. running as a service) — skip colored logging.
                    Console.WriteLine($"{status} {ctx.Request.Method} {url} [{type}]");
                }
            });

            // Configure the HTTP request pipeline.

            // NOTE: no UseHttpsRedirection — game clients connect over plain HTTP
            // and do not follow 307 redirects. TLS termination (if wanted) belongs
            // on a reverse proxy in front of this server.


            app.MapControllers();

            app.Run();
        }
    }
}
