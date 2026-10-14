using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeluxeNET.Data
{
    /// <summary>
    /// A player's personal webpage on rec.net (GeoCities-style). The player
    /// writes raw HTML via the API; it is sanitized at render time and wrapped
    /// in a DeluxNet-branded template. One page per account.
    /// </summary>
    public class PlayerPage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>FK to Accounts. Unique — one page per player.</summary>
        public long AccountId { get; set; }

        /// <summary>Raw user HTML (max 50KB), sanitized when rendered.</summary>
        [MaxLength(51200)]
        public required string HtmlContent { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
