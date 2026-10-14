using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class progression
    {
        [JsonPropertyName("Id")][Key] public long Id { get; set; }
        [JsonPropertyName("PlayerId")] public required long PlayerId { get; set; }
        [JsonPropertyName("Level")] public int Level { get; set; } = 1;
        [JsonPropertyName("XP")] public int XP { get; set; } = 0;
    }
}
