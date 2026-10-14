# DeluxNet

A private **Rec Room** server — run your own Rec Room community on your own hardware, under your own rules.

Built by the **Nextnox** team. ASP.NET Core 9 + MySQL/MariaDB + SignalR.

## What it does

- **Game backend** — accounts, authentication, matchmaking, rooms, avatars, progression, friends, chat, and 30+ API endpoints for Rec Room clients
- **Nameserver** — game clients discover your server automatically
- **Community website** — dark-themed site with live stats, room browser, player profiles, and search
- **Player webpages** — everyone gets a personal page at `rec.net/~username`
- **Volunteer hosting** — anyone can register their machine as a public server; players pick from a live server browser
- **Built-in DNS** — `rec.net` and subdomains resolve to your server; everything else forwards to normal internet, so consoles, phones, and Quest headsets join with zero client mods
- **Load management** — per-IP rate limiting, overload protection, live `/api/health` metrics

## Quick start (Windows)

1. Download `deluxenet-windows.zip` from the [latest release](../../releases/latest)
2. Extract it anywhere
3. Right-click `DeluxeNET.exe` → **Run as administrator**
4. The first-run wizard installs MariaDB, creates the database, stores secrets in Windows Credential Manager, and starts the server
5. Point your game at it (see below) and play

## Pointing the game at your server

**PC:** run `deluxnet-client-setup.bat` as admin (in the release zip) — reversible hosts-file redirect.

**Consoles / phones / VR:** run `dns.bat` as admin on the server PC, then set the device's DNS to the server PC's LAN IP. Full per-device steps in [`docs/PATCH_GUIDE.md`](docs/PATCH_GUIDE.md).

## For players

- [`docs/SIDELOADING_GUIDE.md`](docs/SIDELOADING_GUIDE.md) — install archived Rec Room builds on Android, Quest, iOS, Pico, PS4, Rift, PC
- [`docs/PATCH_GUIDE.md`](docs/PATCH_GUIDE.md) — connect your game to a DeluxNet server
- [`docs/VOLUNTEER_HOSTING.md`](docs/VOLUNTEER_HOSTING.md) — host a public server for the community
- [`docs/WEBPAGE_HOSTING.md`](docs/WEBPAGE_HOSTING.md) — your personal `rec.net/~you` page

## For server operators

- [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) — install, firewall, backup, rollback, updates
- [`docs/LOAD_MANAGEMENT.md`](docs/LOAD_MANAGEMENT.md) — rate limits and health checks
- [`docs/ERROR_AND_COMPATIBILITY_REPORT.md`](docs/ERROR_AND_COMPATIBILITY_REPORT.md) — known issues and platform notes
- [`docs/VERIFICATION_REPORT.md`](docs/VERIFICATION_REPORT.md) — test results

## Building from source

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```bash
# Linux
./build.sh            # or: dotnet publish -c Release

# Windows (one line)
build.bat             # publishes self-contained win-x64 single-file exe
```

On first run without a database, the built-in wizard walks through setup (Windows) — on Linux, configure `ConnectionStrings:Default` and `Jwt:Key` first.

## Configuration

All settings live in `appsettings.json` (or environment variables):

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:Default` | MariaDB/MySQL connection string |
| `Jwt:Key` | Token signing key (32+ chars, auto-generated if missing) |
| `Public:BaseUrl` | Public URL clients use |
| `Dns:Enabled` | Built-in DNS server (default `false`) |
| `Dns:Records` | Local DNS overrides, e.g. `{"rec.net": "auto"}` |
| `Volunteers:Enabled` | Volunteer server registry |
| `Load:MaxConcurrentRequests` | Overload protection threshold |

## License

Private project of the Nextnox team. Rec Room is a trademark of Against Gravity Corp — this is an independent community server, not affiliated with or endorsed by them.
