using System.Text.Json.Serialization;

namespace DeluxeNET.Jsons
{
    public class avatarItem
    {
        [JsonPropertyName("AvatarItemDesc")] public string AvatarItemDesc { get; set; }
        [JsonPropertyName("AvatarItemType")] public int AvatarItemType { get; set; }
        [JsonPropertyName("PlatformMask")] public int PlatformMask { get; set; }
        [JsonPropertyName("FriendlyName")] public string FriendlyName { get; set; }
        [JsonPropertyName("Tooltip")] public string Tooltip { get; set; }
        [JsonPropertyName("Rarity")] public int Rarity { get; set; }
        [JsonPropertyName("TagList")] public string TagList { get; set; }
        [JsonPropertyName("AvatarItemId")] public int AvatarItemId { get; set; }
        [JsonPropertyName("IsBaseAvatarItem")] public bool IsBaseAvatarItem { get; set; }
        [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; }
        [JsonPropertyName("ThumbnailImage")] public string ThumbnailImage { get; set; }
    }
}
