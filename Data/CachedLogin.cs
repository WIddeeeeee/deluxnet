using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class CachedLogin
    {
        [JsonPropertyName("Id")] [Key] public long Id { get; set; }
        [JsonPropertyName("platform")] public required int platform { get; set; }
        [JsonPropertyName("platformId")] public required string platformId { get; set; }
        [JsonPropertyName("accountId")] public required long accountId { get; set; }
        [JsonPropertyName("lastLoginTime")] public required DateTime lastLoginTime { get; set; }
        [JsonPropertyName("requirePassword")] public required bool requirePassword { get; set; }
    }
}
