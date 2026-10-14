using System.Text.Json.Serialization;

namespace DeluxeNET.Models.ServerConfiguration
{
    public class NameServerModel
    {
        [JsonPropertyName("Accounts")] public required string Accounts { get; set; }
        [JsonPropertyName("API")] public required string API { get; set; }
        [JsonPropertyName("Auth")] public required string Auth { get; set; }
        [JsonPropertyName("BugReporting")] public required string BugReporting { get; set; }
        [JsonPropertyName("Cards")] public required string Cards { get; set; }
        [JsonPropertyName("CDN")] public required string CDN { get; set; }
        [JsonPropertyName("Chat")] public required string Chat { get; set; }
        [JsonPropertyName("Clubs")] public required string Clubs { get; set; }
        [JsonPropertyName("CMS")] public required string CMS { get; set; }
        [JsonPropertyName("Commerce")] public required string Commerce { get; set; }
        [JsonPropertyName("Data")] public required string Data { get; set; }
        [JsonPropertyName("DataCollection")] public required string DataCollection { get; set; }
        [JsonPropertyName("Discovery")] public required string Discovery { get; set; }
        [JsonPropertyName("Econ")] public required string Econ { get; set; }
        [JsonPropertyName("GameLogs")] public required string GameLogs { get; set; }
        [JsonPropertyName("Geo")] public required string Geo { get; set; }
        [JsonPropertyName("Images")] public required string Images { get; set; }
        [JsonPropertyName("Leaderboard")] public required string Leaderboard { get; set; }
        [JsonPropertyName("Link")] public required string Link { get; set; }
        [JsonPropertyName("Lists")] public required string Lists { get; set; }
        [JsonPropertyName("Matchmaking")] public required string Matchmaking { get; set; }
        [JsonPropertyName("Moderation")] public required string Moderation { get; set; }
        [JsonPropertyName("Notifications")] public required string Notifications { get; set; }
        [JsonPropertyName("PlatformNotifications")] public required string PlatformNotifications { get; set; }
        [JsonPropertyName("PlayerSettings")] public required string PlayerSettings { get; set; }
        [JsonPropertyName("RoomComments")] public required string RoomComments { get; set; }
        [JsonPropertyName("Rooms")] public required string Rooms { get; set; }
        [JsonPropertyName("Storage")] public required string Storage { get; set; }
        [JsonPropertyName("Strings")] public required string Strings { get; set; }
        [JsonPropertyName("StringsCDN")] public required string StringsCDN { get; set; }
        [JsonPropertyName("Studio")] public required string Studio { get; set; }
        [JsonPropertyName("Thorn")] public required string Thorn { get; set; }
        [JsonPropertyName("Videos")] public required string Videos { get; set; }
        [JsonPropertyName("WWW")] public required string WWW { get; set; }
        [JsonPropertyName("RecNetStatus")] public string RecNetStatus { get; set; } = null;


    }
}
