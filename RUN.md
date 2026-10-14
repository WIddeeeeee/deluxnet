# Running DeluxeNET (Rec Room 2025 Server)

> **Want the zero-hassle version?** Grab `deluxenet-allinone.zip` instead:
> single-file server, no .NET install needed — just `bash install.sh`, then
> `./run.sh`. (The guide below is for building from source.)

## What you need
- **.NET 9 SDK** — https://dotnet.microsoft.com/download/dotnet/9.0
- **MariaDB or MySQL** — `sudo apt install mariadb-server` (Ubuntu)

## Setup
1. Create the database (it can be empty — migrations apply automatically on boot):
   ```sql
   CREATE DATABASE deluxenet CHARACTER SET utf8mb4;
   CREATE USER 'deluxe'@'localhost' IDENTIFIED BY 'pick-a-password';
   GRANT ALL ON deluxenet.* TO 'deluxe'@'localhost';
   ```
2. Edit `appsettings.json`:
   - `ConnectionStrings:Default` → `server=localhost;database=deluxenet;user=deluxe;password=pick-a-password;`
   - `Jwt:Key` → any random secret **at least 32 characters** (or set the `DELUXE_JWT_KEY` env var instead)
   - `Public:BaseUrl` → your public URL, e.g. `https://play.example.com`
   - `Admin:AccountIds` → your account IDs (first registered account is ID 1), e.g. `[1]`
   - `Steam:ApiKey` → your Steam Web API key. While it is still the placeholder,
     Steam ticket validation is skipped (dev mode) so login works for testing.
3. Run it:
   ```bash
   dotnet run
   ```
   First boot creates all tables automatically. The API listens on the ports in
   `Properties/launchSettings.json` (default https://localhost:5001).

## Local rec.net-style website
The server also hosts a community website at **`/web/`** (e.g.
`https://your-host/web/index.html`) — a local tribute to recroom.network:
- **Home** — orange-on-dark navbar, announcement banner, 3-card hero
  (Creator Hub / Download / Event), News·Events·Rooms pills, "Featured Rooms"
  sidebar + live activity feed
- **Rooms Hot List** — 3-column grid of room cards (^name, @creator, cover,
  cheers + visits), with filter
- **Room page** — gradient cover, sub-rooms, visit/cheer/favorite stats
- **Players** — username search; **player pages** with profile, bio, level/XP
- **Creator Hub** — sub-nav (Stats, News, Learn, Earn, Connect, Resources),
  promo cards, news grid, live server status

The game name-server stays on `GET /` untouched — the site lives under `/web/`
so clients keep working. Two small public endpoints power it:
`GET /api/stats` (counts) and `GET /account/search?username=`.

## Pointing a client at it
Map the official hostnames to your server (hosts file) and terminate TLS with a
reverse proxy (see the main README's client-redirect notes). The name-server
endpoint `GET /` tells clients where every service lives — it uses your
`Public:BaseUrl`.

## Key endpoints
| Method | Path | What |
|---|---|---|
| GET | `/` | Name server (service URLs) |
| POST | `/connect/token` | Steam login → JWT (dev bypass until Steam key set) |
| POST | `/account/register` | Create account |
| GET | `/account/me` | Own profile (Bearer token) |
| POST | `/matchmake/none`, `/matchmake/{room}`, `/goto/room/{room}` | Matchmaking |
| GET | `/roomserver/rooms?name=`, `/roomserver/rooms/hot` | Room discovery |
| GET | `/api/versioncheck/v4?v=` | Version check |
| WS | `/hub/v1` | SignalR presence hub |

## Fixes applied to this build (vs the original upload)
1. **JWT key** was `"CHANGE ME!"` (10 bytes) — HMAC-SHA256 needs ≥128 bits, so
   the server crashed on the first token. Now read from `Jwt:Key` / env with a
   secure random fallback.
2. **No account creation existed** — added `POST /account/register`.
3. **Login needed a real Steam key** — dev bypass until `Steam:ApiKey` is set.
4. **Matchmaking 500'd** — SignalR hub was injected as a singleton (`Clients`
   null); now broadcasts via `IHubContext`.
5. **Matchmaking 500'd again** — EF object cycle in JSON (`heartbeat ↔
   roomInstance`); global `ReferenceHandler.IgnoreCycles` + `[JsonIgnore]`.
6. **`GET /account/{id}/bio` crashed** on unknown players (now 404).
7. **Token leaked to console logs** in `GET /account/me` (header dump removed).
8. **Name-server URL hardcoded** to `https://localhost` — now `Public:BaseUrl`.
9. **Migrations needed a live DB at design time** — added a design-time factory;
   **migrations now auto-apply on boot**.
10. **Avatar `GET v2` returned a fake object** with hardcoded `Id=67` instead of
    the created row — now re-queries the real row.
11. **Avatar `POST v2/set` was a no-op stub** — now actually stores the outfit
    (via a DTO; binding the entity 400'd on the required `accountId`).
12. **Config `GET v2` returned double-encoded JSON** (a JSON string instead of an
    object) and 500'd when the file was missing — now serves proper JSON / 404.
13. **Bulk endpoints had no result cap** (`account/bulk`, `players/.../bulk`,
    `playerReputation/.../bulk`) — capped at 100.
14. **`goto/room/{room}` 500'd on fresh DBs** ("Missing RoomId=1 template") —
    now auto-creates the template row.
15. **New room instances relied on EF cascade** to be inserted — now added
    explicitly in both matchmake paths.
16. **`PUT /playersettings` 500'd** on non-form posts and accepted empty keys —
    now validated.
17. **Missing JSON files 500'd** (`avatar/v4/items`, `communityboard/v2/current`)
    — now return empty lists.
18. **Removed a stray `Console.WriteLine("")`** and a bogus
    `System.Reflection.Metadata.Ecma335` using in the avatar controller.
19. **`RoomRole.AccountId` was `int`** while account IDs are `long` — widened.
11. **Migrations needed a live DB at design time** — added a design-time factory;
   **migrations now auto-apply on boot**.
