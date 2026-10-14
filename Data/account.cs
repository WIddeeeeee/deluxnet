using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class account
    {
        [JsonPropertyName("accountId")][Key] public long accountId { get; set; }
        [JsonPropertyName("username")] public required string username { get; set; }
        [JsonPropertyName("displayName")] public required string displayName { get; set; }
        [JsonPropertyName("profileImage")] public string profileImage { get; set; } = "defaultpfp.png";
        [JsonPropertyName("bannerImage")] public string bannerImage { get; set; } = "defaultbanner.png";
        [JsonPropertyName("isJunior")] public bool isJunior { get; set; } = false;
        [JsonPropertyName("bio")] public string bio { get; set; } = string.Empty;
        [JsonPropertyName("platforms")] public int platforms { get; set; } = 0;
        [JsonPropertyName("personalPronouns")] public int personalPronouns { get; set; } = 0;
        [JsonPropertyName("identityFlags")] public int identityFlags { get; set; } = 0;
        [JsonPropertyName("createdAt")] public DateTime createdAt { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("isMetaPlatformBlocked")] public bool isMetaPlatformBlocked { get; set; } = false;
    }
}
