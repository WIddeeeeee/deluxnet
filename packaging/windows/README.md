# DeluxeNET — Windows Server (.exe)

No .NET install needed — it's all inside the .exe.

## Quick start

1. Unzip anywhere, e.g. `C:\DeluxeNET`
2. Double-click **`DeluxeNET.exe`**
   - First run sets everything up by itself: installs MariaDB if it's missing,
     creates the database and user, and saves the secrets in Windows
     Credential Manager (never in a text file)
   - Just answer its questions — it'll ask for the MariaDB root password once
3. Leave the window open. Done.

Open **http://localhost:5118/web/index.html** for the community site.
Point your game client at `http://YOUR-PC-IP:5118`.

To change or remove a stored secret: Windows Settings → Accounts →
Credential Manager → Windows Credentials → `DeluxeNET/DbPassword`.

## Option A: Host a DNS — for Quest, Switch, phones (recommended)

Your server can be its own DNS server. Player devices set their DNS to your
PC's IP; then `rec.net` resolves to YOUR server. Every other domain forwards
to normal internet DNS untouched, so devices work normally otherwise.

1. Double-click **`dns.bat`** (asks for admin — DNS needs port 53)
   - Same first-run password prompt as `run.bat`, stored in Credential Manager
   - The console prints your PC's LAN IP, e.g.:
     `Players: set device DNS to 192.168.1.50`
2. On each player device, set its DNS server to that IP:
   - **Nintendo Switch:** System Settings → Internet → Internet Settings →
     your network → Change Settings → DNS Settings → Manual →
     Primary DNS = your PC's IP
   - **iPhone / Android:** Wi-Fi settings → your network → Configure DNS →
     Manual → your PC's IP
   - **Quest:** no built-in DNS setting — sideload a DNS-changer app
     (via SideQuest) and point it at your PC's IP
   - **PC:** set DNS manually, or just run `join-server.bat` (hosts file)
3. Done. `http://rec.net/` opens your community site, the game reaches your
   server. Run `run.bat` instead of `dns.bat` anytime you don't need the DNS.

To change which names resolve to your server, edit the `Dns` section in
`appsettings.json` (`"auto"` = this PC's LAN IP, or put an explicit IP).
`Web:Hostnames` controls which hostnames show the website instead of the
game name-server JSON.

## Option B: Wi-Fi hotspot (Windows PCs)

1. Right-click **`hotspot.bat`** → **Run as administrator** → type `start`
   - Creates the "Nextnox" Wi-Fi hotspot (password: `nextnox123`)
   - Forwards port 80 → 5118 so `http://rec.net/` works
2. Start the server with `run.bat`
3. On each **player's PC**: connect to "Nextnox" Wi-Fi, run **`join-server.bat`**
   (asks before changing anything) — it points `rec.net` at your server.
   Reversible: `join-server.bat remove`

Note: a Windows hotspot can't override DNS for connected devices on its own,
so consoles/VR need Option A (DNS) — e.g. run the DNS on this PC and have
Switch players set manual DNS to your PC's IP. Stop the hotspot later with
`hotspot.bat stop` (also as administrator).

## Files

- `DeluxeNET.exe` — the server (self-contained, ~100 MB). Double-click it:
  first run installs MariaDB if needed, creates the database, and starts.
- `run.bat` — starts the server (same as double-clicking the exe)
- `dns.bat` — starts the server WITH the DNS server (asks for admin)
- `install.bat` — installs MariaDB on its own (optional; the exe does this too)
- `setup-db.sql` — manual database setup (optional; the exe does this too)
- `hotspot.bat` — Wi-Fi hotspot start/stop (admin)
- `join-server.bat` / `join-server.ps1` — player-side rec.net redirect (reversible)
- `wwwroot/` — the website served at `/web/`
- `Jsons/` — game config/data files

Database tables are created automatically on first boot.
To change the port, edit `$port` at the bottom of `run.ps1`.
