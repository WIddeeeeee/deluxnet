using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DeluxeNET.Data
{
    public class Room
    {
        [Key]
        [JsonPropertyName("RoomId")]
        public long RoomId { get; set; }

        [JsonPropertyName("Accessibility")]
        public int Accessibility { get; set; }

        [JsonPropertyName("CloningAllowed")]
        public bool CloningAllowed { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("CreatorAccountId")]
        public int CreatorAccountId { get; set; }

        [JsonPropertyName("CustomWarning")]
        public string CustomWarning { get; set; } = string.Empty;

        [JsonPropertyName("DataBlob")]
        [NotMapped]
        public object DataBlob { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("DisableMicAutoMute")]
        public bool DisableMicAutoMute { get; set; }

        [JsonPropertyName("DisableRoomComments")]
        public bool DisableRoomComments { get; set; }

        [JsonPropertyName("EncryptVoiceChat")]
        public bool EncryptVoiceChat { get; set; }

        [JsonPropertyName("ImageName")]
        public string ImageName { get; set; } = string.Empty;

        [JsonPropertyName("IsDeveloperOwned")]
        public bool IsDeveloperOwned { get; set; }

        [JsonPropertyName("IsDorm")]
        public bool IsDorm { get; set; }

        [JsonPropertyName("IsRRO")]
        public bool IsRRO { get; set; }

        [JsonPropertyName("LoadScreenLocked")]
        public bool LoadScreenLocked { get; set; }

        [JsonPropertyName("LoadScreens")]
        [NotMapped]
        public List<object> LoadScreens { get; set; }

        [JsonPropertyName("MaxPlayerCalculationMode")]
        public int MaxPlayerCalculationMode { get; set; }

        [JsonPropertyName("MaxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonPropertyName("MinLevel")]
        public int MinLevel { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("PromoExternalContent")]
        [NotMapped]
        public List<object> PromoExternalContent { get; set; }

        [JsonPropertyName("PromoImages")]
        [NotMapped]
        public List<object> PromoImages { get; set; }

        [JsonPropertyName("Roles")]
        public List<RoomRole> Roles { get; set; } = new();

        [JsonPropertyName("State")]
        public int State { get; set; }

        [JsonPropertyName("Stats")]
        public RoomStats Stats { get; set; }

        [JsonPropertyName("SupportsJuniors")]
        public bool SupportsJuniors { get; set; }

        [JsonPropertyName("SupportsLevelVoting")]
        public bool SupportsLevelVoting { get; set; }

        [JsonPropertyName("SupportsMobile")]
        public bool SupportsMobile { get; set; }

        [JsonPropertyName("SupportsQuest2")]
        public bool SupportsQuest2 { get; set; }

        [JsonPropertyName("SupportsScreens")]
        public bool SupportsScreens { get; set; }

        [JsonPropertyName("SupportsTeleportVR")]
        public bool SupportsTeleportVR { get; set; }

        [JsonPropertyName("SupportsVRLow")]
        public bool SupportsVRLow { get; set; }

        [JsonPropertyName("SupportsWalkVR")]
        public bool SupportsWalkVR { get; set; }

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("Version")]
        public int Version { get; set; }

        [JsonPropertyName("WarningMask")]
        public int WarningMask { get; set; }

        [JsonPropertyName("SubRooms")]
        public List<SubRoom> SubRooms { get; set; } = new();
    }

    public class RoomRole
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Room")]
        [JsonPropertyName("RoomId")]
        public long RoomId { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }

        [JsonPropertyName("AccountId")]
        public long AccountId { get; set; }

        [JsonPropertyName("InvitedRole")]
        public int InvitedRole { get; set; }

        [JsonPropertyName("Role")]
        public int Role { get; set; }
    }

    public class RoomStats
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Room")]
        [JsonPropertyName("RoomId")]
        public long RoomId { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }

        [JsonPropertyName("CheerCount")]
        public int CheerCount { get; set; }

        [JsonPropertyName("FavoriteCount")]
        public int FavoriteCount { get; set; }

        [JsonPropertyName("VisitCount")]
        public int VisitCount { get; set; }

        [JsonPropertyName("VisitorCount")]
        public int VisitorCount { get; set; }
    }

    public class SubRoom
    {
        [Key]
        [JsonPropertyName("SubRoomId")]
        public int SubRoomId { get; set; }

        [ForeignKey("Room")]
        [JsonPropertyName("RoomId")]
        public long RoomId { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }

        [JsonPropertyName("Accessibility")]
        public int Accessibility { get; set; }

        [JsonPropertyName("CurrentSave")]
        public SubRoomSave CurrentSave { get; set; }

        [JsonIgnore]
        public List<SubRoomSave> SubRoomSaves { get; set; } = new();

        [JsonPropertyName("IsSandbox")]
        public bool IsSandbox { get; set; }

        [JsonPropertyName("MaxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("UnitySceneId")]
        public string UnitySceneId { get; set; } = string.Empty;
    }

    public class SubRoomSave
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Room")]
        [JsonPropertyName("RoomId")]
        public long RoomId { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }

        [ForeignKey("SubRoom")]
        [JsonPropertyName("SubRoomId")]
        public int SubRoomId { get; set; }

        [JsonIgnore]
        public SubRoom SubRoom { get; set; }

        [JsonPropertyName("SubRoomDataSaveId")]
        public int SubRoomDataSaveId { get; set; }

        [JsonPropertyName("DataBlob")]
        public string DataBlob { get; set; } = string.Empty;

        [JsonPropertyName("SavedByAccountId")]
        public int SavedByAccountId { get; set; }

        [JsonPropertyName("SavedOnPlatform")]
        [NotMapped]
        public object SavedOnPlatform { get; set; }

        [JsonPropertyName("SavedOnDeviceClass")]
        [NotMapped]
        public object SavedOnDeviceClass { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("UnityAssetId")]
        public string UnityAssetId { get; set; } = string.Empty;
    }
}