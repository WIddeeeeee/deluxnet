using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class avatar
    {
        [JsonPropertyName("Id")][Key] public long Id { get; set; }
        [JsonPropertyName("accountId")] public required long? accountId { get; set; }
        [JsonPropertyName("OutfitSelections")] public string OutfitSelections { get; set; } = string.Empty;
        [JsonPropertyName("FaceFeatures")] public string FaceFeatures { get; set; } = string.Empty;
        [JsonPropertyName("SkinColor")] public string SkinColor { get; set; } = string.Empty;
        [JsonPropertyName("HairColor")] public string HairColor { get; set; } = string.Empty;
    }
}
