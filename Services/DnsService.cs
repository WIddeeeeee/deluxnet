using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace DeluxeNET.Services
{
    /// <summary>
    /// Optional DNS server for private-server play (opt-in, off by default).
    ///
    /// Serves operator-configured local records — e.g. rec.net -> this machine —
    /// and forwards every other query to the upstream resolver untouched, so
    /// devices keep normal internet. Players opt in by setting their device's
    /// DNS server to this machine's IP (Switch/phone network settings, or a
    /// DNS-changer app on Quest which has no built-in DNS setting).
    ///
    /// Config (appsettings or env Dns__*):
    ///   Dns:Enabled  = false
    ///   Dns:Upstream = "8.8.8.8"
    ///   Dns:Records  = { "rec.net": "auto" }   ("auto" = this machine's LAN IP,
    ///                                          or put an explicit IP)
    ///
    /// Binding UDP port 53 needs admin/root. If the port can't be bound, it
    /// logs an error and the game server keeps running without DNS.
    /// </summary>
    public class DnsService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DnsService> _log;

        public DnsService(IConfiguration config, ILogger<DnsService> log)
        {
            _config = config;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var upstream = _config["Dns:Upstream"] ?? "8.8.8.8";
            var records = (_config.GetSection("Dns:Records").Get<Dictionary<string, string>>()
                ?? new Dictionary<string, string>())
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToDictionary(
                    kv => kv.Key.Trim().TrimEnd('.').ToLowerInvariant(),
                    kv => kv.Value.Trim().ToLowerInvariant() == "auto" ? GetLanIp() : kv.Value.Trim());

            UdpClient udp;
            try
            {
                udp = new UdpClient(53);
            }
            catch (Exception ex)
            {
                _log.LogError("DNS server could not bind UDP port 53 ({Msg}). Run as admin/root and make sure no other DNS server is running. Continuing without DNS.", ex.Message);
                return;
            }

            using (udp)
            {
                foreach (var kv in records)
                    _log.LogInformation("DNS: {Name} (+subdomains) -> {Ip}  [local record]", kv.Key, kv.Value);
                _log.LogInformation("DNS server live on port 53 (upstream {Up}). Players: set device DNS to {Lan}.",
                    upstream, GetLanIp());

                while (!stoppingToken.IsCancellationRequested)
                {
                    UdpReceiveResult req;
                    try { req = await udp.ReceiveAsync(stoppingToken); }
                    catch (OperationCanceledException) { break; }
                    catch { continue; }

                    _ = Task.Run(() => HandleAsync(req, udp, records, upstream), stoppingToken);
                }
            }
        }

        private static string GetLanIp()
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    var ip = ua.Address;
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private static bool MatchesRecord(string name, string record)
        {
            name = name.TrimEnd('.').ToLowerInvariant();
            return name.Length > 0 && (name == record || name.EndsWith("." + record));
        }

        private async Task HandleAsync(UdpReceiveResult req, UdpClient udp,
            Dictionary<string, string> records, string upstream)
        {
            var q = req.Buffer;
            byte[] resp;
            try
            {
                resp = await ResolveAsync(q, records, upstream);
            }
            catch
            {
                resp = BuildError(q, 2); // SERVFAIL
            }

            try { await udp.SendAsync(resp, resp.Length, req.RemoteEndPoint); }
            catch { /* client went away; ignore */ }
        }

        private static async Task<byte[]> ResolveAsync(byte[] q,
            Dictionary<string, string> records, string upstream)
        {
            // Only answer A queries (type 1) for our local records; forward the rest.
            if (q.Length > 12 && GetQType(q) == 1)
            {
                var name = GetQName(q);
                foreach (var kv in records)
                {
                    if (MatchesRecord(name, kv.Key))
                        return BuildAnswer(q, kv.Value);
                }
            }
            return await ForwardAsync(q, upstream);
        }

        private static string GetQName(byte[] q)
        {
            var sb = new StringBuilder();
            int i = 12;
            while (i < q.Length && q[i] != 0)
            {
                int len = q[i++];
                if (i + len > q.Length) break;
                if (sb.Length > 0) sb.Append('.');
                sb.Append(Encoding.ASCII.GetString(q, i, len));
                i += len;
            }
            return sb.ToString();
        }

        private static ushort GetQType(byte[] q)
        {
            int i = 12;
            while (i < q.Length && q[i] != 0) i += q[i] + 1;
            i++; // null terminator
            if (i + 1 >= q.Length) return 0;
            return (ushort)((q[i] << 8) | q[i + 1]);
        }

        private static byte[] BuildAnswer(byte[] q, string ip)
        {
            var resp = new List<byte>();
            resp.Add(q[0]); resp.Add(q[1]);                       // ID
            resp.AddRange(new byte[] { 0x81, 0x80 });            // flags: standard response
            resp.AddRange(new byte[] { 0x00, 0x01 });            // QDCOUNT = 1
            resp.AddRange(new byte[] { 0x00, 0x01 });            // ANCOUNT = 1
            resp.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // NSCOUNT/ARCOUNT = 0

            int i = 12;                                          // copy question verbatim
            while (i < q.Length && q[i] != 0) i += q[i] + 1;
            int qEnd = Math.Min(i + 5, q.Length);
            for (int j = 12; j < qEnd; j++) resp.Add(q[j]);

            resp.AddRange(new byte[] { 0xC0, 0x0C });            // NAME = pointer to QNAME
            resp.AddRange(new byte[] { 0x00, 0x01 });            // TYPE = A
            resp.AddRange(new byte[] { 0x00, 0x01 });            // CLASS = IN
            var ttl = BitConverter.GetBytes((uint)60);           // TTL = 60s
            if (BitConverter.IsLittleEndian) Array.Reverse(ttl);
            resp.AddRange(ttl);
            resp.AddRange(new byte[] { 0x00, 0x04 });            // RDLENGTH = 4
            resp.AddRange(IPAddress.Parse(ip).GetAddressBytes());

            return resp.ToArray();
        }

        private static byte[] BuildError(byte[] q, int rcode)
        {
            var resp = new List<byte>();
            if (q.Length >= 2) { resp.Add(q[0]); resp.Add(q[1]); }
            else resp.AddRange(new byte[] { 0x00, 0x00 });
            resp.AddRange(new byte[] { 0x81, (byte)(0x80 | (rcode & 0x0F)), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            return resp.ToArray();
        }

        private static async Task<byte[]> ForwardAsync(byte[] q, string upstream)
        {
            using var cts = new CancellationTokenSource(2500);
            using var fwd = new UdpClient();
            fwd.Connect(upstream, 53);
            await fwd.SendAsync(q, q.Length);
            var r = await fwd.ReceiveAsync(cts.Token);
            return r.Buffer;
        }
    }
}
