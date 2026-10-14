using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeluxeNET.Data
{
    /// <summary>
    /// A community-volunteered public game server. Volunteers register their
    /// machine, send heartbeats every ~60s to stay listed, and players pick
    /// from the live list. Only the SHA256 hash of the heartbeat token is
    /// stored — the plaintext token is shown once at registration.
    /// </summary>
    public class VolunteerServer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(60)]
        public required string Name { get; set; }

        [MaxLength(253)]
        public required string Host { get; set; }

        public int Port { get; set; }

        public int MaxPlayers { get; set; }

        public int CurrentPlayers { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(120)]
        public string? Contact { get; set; }

        /// <summary>SHA256 hex of the heartbeat token. Never the token itself.</summary>
        [MaxLength(64)]
        public required string TokenHash { get; set; }

        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
