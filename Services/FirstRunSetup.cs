using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector;

namespace DeluxeNET.Services
{
    /// <summary>
    /// Makes DeluxeNET.exe "just work" on first double-click (Windows):
    /// if the database isn't reachable, it walks through installing MariaDB,
    /// creating the database/user, and storing secrets in Windows Credential
    /// Manager. Normal runs skip all of this silently.
    /// </summary>
    public static class FirstRunSetup
    {
        private const string DbSecretName = "DeluxeNET/DbPassword";
        private const string JwtSecretName = "DeluxeNET/JwtKey";

        public static async Task EnsureReadyAsync(ConfigurationManager config)
        {
            var cs = ResolveConnectionString(config);
            if (await TryConnectAsync(cs))
            {
                config["ConnectionStrings:Default"] = cs;
                EnsureJwtKey(config);
                return;
            }

            if (!OperatingSystem.IsWindows() || Console.IsInputRedirected)
                throw new InvalidOperationException(
                    "Cannot reach the database. On Linux, run install.sh first, then check " +
                    "the connection string in .env / appsettings.json.");

            await RunWizardAsync(config);
        }

        private static string ResolveConnectionString(ConfigurationManager config)
        {
            var cs = config.GetConnectionString("Default") ?? "";
            if (IsPlaceholder(cs))
            {
                var saved = WindowsCredentialStore.Read(DbSecretName);
                if (!string.IsNullOrEmpty(saved))
                    cs = $"server=localhost;database=deluxenet;user=deluxe;password={saved};";
            }
            return cs;
        }

        private static bool IsPlaceholder(string cs) =>
            string.IsNullOrWhiteSpace(cs) ||
            cs.Contains("YOUR_SERVER_IP") ||
            cs.Contains("redacted");

        private static void EnsureJwtKey(ConfigurationManager config)
        {
            var jwt = config["Jwt:Key"];
            if (!string.IsNullOrWhiteSpace(jwt) && !jwt.Contains("REPLACE_WITH") &&
                Encoding.UTF8.GetByteCount(jwt) >= 16)
                return;

            var saved = WindowsCredentialStore.Read(JwtSecretName);
            if (!string.IsNullOrWhiteSpace(saved) && Encoding.UTF8.GetByteCount(saved) >= 16)
            {
                config["Jwt:Key"] = saved;
                return;
            }

            var fresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            WindowsCredentialStore.Write(JwtSecretName, fresh);
            config["Jwt:Key"] = fresh;
        }

        private static async Task<bool> TryConnectAsync(string cs)
        {
            if (string.IsNullOrWhiteSpace(cs)) return false;
            try
            {
                using var conn = new MySqlConnection(cs);
                using var cts = new CancellationTokenSource(6000);
                await conn.OpenAsync(cts.Token);
                return true;
            }
            catch { return false; }
        }

        private static async Task RunWizardAsync(ConfigurationManager config)
        {
            Console.WriteLine();
            Console.WriteLine("=== DeluxeNET first-run setup ===");
            Console.WriteLine("The database isn't reachable yet. I'll set everything up now.");
            Console.WriteLine();

            if (!await MariaDbServiceExistsAsync() && !await TryRootNoPasswordAsync())
            {
                Console.Write("MariaDB doesn't seem to be installed. Install it now? [Y/n] ");
                if (ReadYesNo(true) && !await InstallMariaDbAsync())
                {
                    Console.WriteLine();
                    Console.WriteLine("The automatic install didn't work. Install it yourself with:");
                    Console.WriteLine("    winget install MariaDB.Server");
                    Console.WriteLine("(run that in an administrator cmd window, watch for errors)");
                    Console.WriteLine("then re-run this exe.");
                    throw new InvalidOperationException("MariaDB install failed.");
                }
                await WaitForMariaDbServiceAsync();
            }

            var service = await FindServiceNameAsync();
            if (service == null && !await TryRootNoPasswordAsync())
            {
                Console.WriteLine();
                Console.WriteLine("MariaDB still isn't reachable. The install may have failed.");
                Console.WriteLine("Try installing it yourself in an admin cmd window with:");
                Console.WriteLine("    winget install MariaDB.Server");
                Console.WriteLine("then re-run this exe.");
                throw new InvalidOperationException("MariaDB is not installed.");
            }
            if (service != null) StartService(service);

            using var root = await ConnectAsRootAsync();
            if (root == null)
                throw new InvalidOperationException("Setup stopped: couldn't connect to MariaDB as root.");

            var dbPass = GenerateSecret(32);
            foreach (var sql in new[]
            {
                "CREATE DATABASE IF NOT EXISTS deluxenet CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
                $"CREATE USER IF NOT EXISTS 'deluxe'@'localhost' IDENTIFIED BY '{dbPass}';",
                "GRANT ALL PRIVILEGES ON deluxenet.* TO 'deluxe'@'localhost';",
                "FLUSH PRIVILEGES;"
            })
            {
                using var cmd = new MySqlCommand(sql, root);
                await cmd.ExecuteNonQueryAsync();
            }

            var jwt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            WindowsCredentialStore.Write(DbSecretName, dbPass);
            WindowsCredentialStore.Write(JwtSecretName, jwt);
            Console.WriteLine("Secrets saved to Windows Credential Manager. You won't be asked again.");
            Console.WriteLine();

            var cs = $"server=localhost;database=deluxenet;user=deluxe;password={dbPass};";
            config["ConnectionStrings:Default"] = cs;
            config["Jwt:Key"] = jwt;

            if (!await TryConnectAsync(cs))
                throw new InvalidOperationException(
                    "Database was created but I still can't connect. Re-run the exe and try again.");
            Console.WriteLine("Database ready. Starting the server...");
            Console.WriteLine();
        }

        /// <summary>
        /// Installs MariaDB via winget and verifies it actually worked.
        /// Returns false if the install failed so the wizard can bail out
        /// with manual instructions instead of pretending it worked.
        /// </summary>
        private static async Task<bool> InstallMariaDbAsync()
        {
            Console.WriteLine("Installing MariaDB (takes a few minutes — approve the admin prompt if one appears)...");
            try
            {
                var p = Process.Start(new ProcessStartInfo("winget",
                    "install MariaDB.Server --accept-source-agreements --accept-package-agreements")
                { UseShellExecute = true });
                if (p == null)
                {
                    Console.WriteLine("Couldn't start winget.");
                    return false;
                }
                await p.WaitForExitAsync();
                if (p.ExitCode != 0)
                {
                    Console.WriteLine($"The installer exited with code {p.ExitCode} (0 means success).");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't run the installer: {ex.Message}");
                return false;
            }

            if (await FindServiceNameAsync() == null)
            {
                Console.WriteLine("Install finished but no MariaDB service was created.");
                return false;
            }
            Console.WriteLine("MariaDB installed.");
            return true;
        }

        private static async Task WaitForMariaDbServiceAsync()
        {
            Console.WriteLine("Waiting for the MariaDB service to appear (press Enter to skip)...");
            for (var i = 0; i < 15; i++)
            {
                if (await FindServiceNameAsync() != null) return;
                for (var j = 0; j < 20; j++)
                {
                    await Task.Delay(100);
                    try
                    {
                        if (Console.KeyAvailable)
                        {
                            Console.ReadLine(); // consume the line
                            Console.WriteLine("Skipped waiting.");
                            return;
                        }
                    }
                    catch { /* input unavailable; keep waiting */ }
                }
            }
            Console.WriteLine("MariaDB service not found yet — continuing anyway.");
        }

        private static readonly string[] ServiceNames = { "MariaDB", "MySQL" };

        private static async Task<string?> FindServiceNameAsync()
        {
            foreach (var name in ServiceNames)
            {
                try
                {
                    var p = Process.Start(new ProcessStartInfo("sc", $"query {name}")
                    { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
                    if (p == null) continue;
                    var output = await p.StandardOutput.ReadToEndAsync();
                    await p.WaitForExitAsync();
                    if (output.Contains("SERVICE_NAME: " + name, StringComparison.OrdinalIgnoreCase))
                        return name;
                }
                catch { /* try next */ }
            }
            return null;
        }

        private static async Task<bool> MariaDbServiceExistsAsync() =>
            await FindServiceNameAsync() != null;

        private static async Task<bool> TryRootNoPasswordAsync()
        {
            try
            {
                using var conn = new MySqlConnection("server=localhost;user=root;");
                using var cts = new CancellationTokenSource(4000);
                await conn.OpenAsync(cts.Token);
                return true;
            }
            catch { return false; }
        }

        private static void StartService(string name)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo("sc", $"start {name}")
                { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
                p?.WaitForExit(15000);
            }
            catch { /* best effort */ }
        }

        private enum RootConnectResult { Connected, AccessDenied, Unreachable }

        private static async Task<MySqlConnection> ConnectAsRootAsync()
        {
            // Many installs have no root password — try blank first, silently.
            var (r, c) = await TryRootPasswordAsync("");
            if (r == RootConnectResult.Connected) return c!;

            var deniedCount = 0;
            while (true)
            {
                if (r == RootConnectResult.Unreachable)
                {
                    Console.WriteLine("MariaDB doesn't seem to be running — trying to start it...");
                    var svc = await FindServiceNameAsync();
                    if (svc != null) StartService(svc);
                    await Task.Delay(4000);
                    (r, c) = await TryRootPasswordAsync("");
                    if (r == RootConnectResult.Connected) return c!;
                    if (r == RootConnectResult.Unreachable)
                        throw new InvalidOperationException(
                            "Still can't reach MariaDB. Make sure it's installed, then re-run this exe.");
                    continue;
                }

                // AccessDenied from here on.
                deniedCount++;
                if (deniedCount >= 2)
                {
                    Console.Write("Still can't connect as root. Reset the MariaDB root password automatically? [Y/n] ");
                    if (ReadYesNo(true) && await ResetRootPasswordAsync())
                    {
                        Console.WriteLine("Root password cleared. Continuing setup...");
                        (r, c) = await TryRootPasswordAsync("");
                        if (r == RootConnectResult.Connected) return c!;
                    }
                    deniedCount = 0;
                }

                Console.Write("MariaDB root password (just press Enter if you didn't set one): ");
                var pw = ReadPassword();
                Console.WriteLine();
                (r, c) = await TryRootPasswordAsync(pw);
                if (r == RootConnectResult.Connected) return c!;
                if (r == RootConnectResult.Unreachable) continue; // server went away; loop handles it
                Console.WriteLine("Wrong password — try again.");
            }
        }

        private static async Task<(RootConnectResult, MySqlConnection?)> TryRootPasswordAsync(string pw)
        {
            try
            {
                var csb = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    UserID = "root",
                    Password = pw
                };
                var conn = new MySqlConnection(csb.ConnectionString);
                using var cts = new CancellationTokenSource(8000);
                await conn.OpenAsync(cts.Token);
                return (RootConnectResult.Connected, conn);
            }
            catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.AccessDenied)
            {
                return (RootConnectResult.AccessDenied, null);
            }
            catch
            {
                return (RootConnectResult.Unreachable, null);
            }
        }

        /// <summary>
        /// Clears a lost MariaDB root password via --skip-grant-tables.
        /// Stops the service, starts mysqld in reset mode, clears the password,
        /// then restarts the service normally.
        /// </summary>
        private static async Task<bool> ResetRootPasswordAsync()
        {
            var service = await FindServiceNameAsync();
            var mysqld = service != null ? FindMysqldPath(service) : null;
            if (mysqld == null || service == null)
            {
                Console.WriteLine("Couldn't locate the MariaDB install. Reset it manually instead.");
                return false;
            }

            Console.WriteLine("Stopping MariaDB...");
            if (!StopService(service))
            {
                Console.WriteLine("Couldn't stop the MariaDB service — re-run this exe as administrator.");
                return false;
            }

            Console.WriteLine("Starting MariaDB in password-reset mode...");
            var proc = Process.Start(new ProcessStartInfo(mysqld, "--skip-grant-tables")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (proc == null)
            {
                StartService(service);
                return false;
            }

            try
            {
                MySqlConnection? conn = null;
                for (var i = 0; i < 30; i++)
                {
                    try
                    {
                        conn = new MySqlConnection("server=localhost;user=root;");
                        using var cts = new CancellationTokenSource(3000);
                        await conn.OpenAsync(cts.Token);
                        break;
                    }
                    catch { conn?.Dispose(); conn = null; await Task.Delay(1000); }
                }
                if (conn == null)
                {
                    Console.WriteLine("Password-reset mode didn't start.");
                    return false;
                }

                using (conn)
                {
                    foreach (var sql in new[]
                    {
                        "FLUSH PRIVILEGES;",
                        "ALTER USER 'root'@'localhost' IDENTIFIED BY '';",
                        "FLUSH PRIVILEGES;"
                    })
                    {
                        using var cmd = new MySqlCommand(sql, conn);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine("Root password cleared.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset failed: {ex.Message}");
                return false;
            }
            finally
            {
                try { proc.Kill(); } catch { }
                proc.WaitForExit(10000);
                Console.WriteLine("Restarting MariaDB normally...");
                StartService(service);
                await Task.Delay(3000);
            }
        }

        private static string? FindMysqldPath(string serviceName)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo("sc", $"qc {serviceName}")
                { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
                if (p != null)
                {
                    var output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    var m = System.Text.RegularExpressions.Regex.Match(
                        output, "\"([^\"]*mysqld\\.exe)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (m.Success && File.Exists(m.Groups[1].Value)) return m.Groups[1].Value;
                }
            }
            catch { /* fall through */ }

            try
            {
                foreach (var dir in Directory.GetDirectories(@"C:\Program Files", "MariaDB*"))
                {
                    var candidate = Path.Combine(dir, "bin", "mysqld.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            catch { /* fall through */ }
            return null;
        }

        private static bool StopService(string name)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo("sc", $"stop {name}")
                { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
                if (p == null) return false;
                p.WaitForExit(30000);
                for (var i = 0; i < 20; i++)
                {
                    var q = Process.Start(new ProcessStartInfo("sc", $"query {name}")
                    { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
                    if (q != null)
                    {
                        var output = q.StandardOutput.ReadToEnd();
                        q.WaitForExit();
                        if (output.Contains("STOPPED")) return true;
                    }
                    Task.Delay(1000).Wait();
                }
            }
            catch { /* fall through */ }
            return false;
        }

        private static bool ReadYesNo(bool defaultYes)
        {
            var line = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (line == "") return defaultYes;
            return line.StartsWith("y");
        }

        private static string ReadPassword()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return sb.ToString();
        }

        private static string GenerateSecret(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append(alphabet[b % alphabet.Length]);
            return sb.ToString();
        }
    }
}
