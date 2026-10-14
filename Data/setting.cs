using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class setting
    {
        [JsonPropertyName("Id")][Key] public long Id { get; set; }
        [JsonPropertyName("accountId")] public required long? accountId { get; set; }
        [JsonPropertyName("Key")] public required string Key { get; set; }
        [JsonPropertyName("Value")] public required string Value { get; set; }
    }
}
