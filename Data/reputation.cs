using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class reputation
    {
        [JsonPropertyName("Id")][Key] public long Id { get; set; }
        [JsonPropertyName("accountId")] public required long accountId { get; set; }
        [JsonPropertyName("IsCheerful")] public bool IsCheerful { get; set; } = false;
        [JsonPropertyName("Noteriety")] public float Noteriety { get; set; } = 0.0f;
        [JsonPropertyName("SelectedCheer")] public int SelectedCheer { get; set; } = 0;
        [JsonPropertyName("CheerCredit")] public int CheerCredit { get; set; } = 20;
        [JsonPropertyName("CheerGeneral")] public int CheerGeneral { get; set; } = 0;
        [JsonPropertyName("CheerHelpful")] public int CheerHelpful { get; set; } = 0;
        [JsonPropertyName("CheerCreative")] public int CheerCreative { get; set; } = 0;
        [JsonPropertyName("CheerGreatHost")] public int CheerGreatHost { get; set; } = 0;
        [JsonPropertyName("CheerSportsman")] public int CheerSportsman { get; set; } = 0;
    }
}
