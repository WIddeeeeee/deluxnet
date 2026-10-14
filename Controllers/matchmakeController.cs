using DeluxeNET.Data;
using DeluxeNET.Hubs;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DeluxeNET.Controllers
{
    [Route("/")]
    [ApiController]
    public class matchmakeController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> _hub;

        public matchmakeController(AppDbContext db, jwt _jwt, Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> hub)
        {
            _db = db;
            this._jwt = _jwt;
            _hub = hub;
        }

        [HttpPost("matchmake/none")]
        public async Task<ActionResult> MatchmakeNone()
        {
            var token = HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
                return Unauthorized();

            token = token["Bearer ".Length..].Trim();

            long? playerIdNullable;

            try
            {
                playerIdNullable = _jwt.VerifyToken(token);
            }
            catch
            {
                return Unauthorized();
            }

            if (!playerIdNullable.HasValue)
                return Unauthorized();

            long playerId = playerIdNullable.Value;

            var heartbeat = await _db.Heartbeats
                .FirstOrDefaultAsync(h => h.PlayerId == playerId);

            if (heartbeat == null)
            {
                heartbeat = new Heartbeat
                {
                    PlayerId = playerId,
                    IsOnline = true,
                    LastOnline = DateTime.UtcNow,
                    StatusVisibility = 0,
                    Platform = 0,
                    DeviceClass = 0,
                    VrMovementMode = 0,
                    AppVersion = "20230406"
                };

                _db.Heartbeats.Add(heartbeat);
            }

            heartbeat.RoomInstance = null;
            heartbeat.RoomInstanceId = null;
            heartbeat.IsOnline = true;
            heartbeat.LastOnline = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("Notification", JsonSerializer.Serialize(new
            {
                Id = "PresenceUpdate",
                Msg = heartbeat
            }));

            var newtoken = _jwt.GenerateToken(playerId.ToString());

            return Ok(new
            {
                access_token = newtoken,
                error_description = "",
                error = "",
                refresh_token = newtoken,
                key = ""
            });
        }

        [HttpPost("matchmake/{room}")]
        public async Task<ActionResult> Matchmake([FromRoute] string room)
        {
            var token = HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
                return Unauthorized();

            token = token["Bearer ".Length..].Trim();

            long? playerIdNullable;

            try
            {
                playerIdNullable = _jwt.VerifyToken(token);
            }
            catch
            {
                return Unauthorized();
            }

            if (!playerIdNullable.HasValue)
                return Unauthorized();

            long playerId = playerIdNullable.Value;

            var heartbeat = await _db.Heartbeats
                .Include(h => h.RoomInstance)
                .FirstOrDefaultAsync(h => h.PlayerId == playerId);

            if (heartbeat == null)
            {
                heartbeat = new Heartbeat
                {
                    PlayerId = playerId,
                    IsOnline = true,
                    LastOnline = DateTime.UtcNow,
                    StatusVisibility = 0,
                    Platform = 0,
                    DeviceClass = 0,
                    VrMovementMode = 0,
                    AppVersion = "20230406"
                };

                _db.Heartbeats.Add(heartbeat);
            }

            var candidates = await _db.Heartbeats
                .Include(h => h.RoomInstance)
                .Where(h =>
                    h.RoomInstance != null &&
                    h.RoomInstance.Name == room &&
                    !h.RoomInstance.IsPrivate)
                .ToListAsync();

            var roomInstance = candidates
                .GroupBy(h => h.RoomInstanceId)
                .Select(g => new
                {
                    Room = g.First().RoomInstance,
                    Count = g.Count()
                })
                .FirstOrDefault(x =>
                    x.Room != null &&
                    x.Count < x.Room.MaxCapacity)?
                .Room;

            if (roomInstance == null)
            {
                roomInstance = new RoomInstance
                {
                    Name = room,
                    RoomId = 0,
                    SubRoomId = 0,
                    Location = "76d98498-60a1-430c-ab76-b54a29b7a163",
                    RoomInstanceType = 0,
                    PhotonRegionId = "us",
                    PhotonRegion = "us",
                    PhotonRoomId = Guid.NewGuid().ToString(),
                    MaxCapacity = 10,
                    IsFull = false,
                    IsPrivate = false,
                    IsInProgress = false,
                    MatchmakingPolicy = 0
                };
                // FIX: add explicitly instead of relying on EF cascade from heartbeat.
                _db.Set<RoomInstance>().Add(roomInstance);
            }

            heartbeat.RoomInstance = roomInstance;
            heartbeat.RoomInstanceId = roomInstance.RoomInstanceId;
            heartbeat.IsOnline = true;
            heartbeat.LastOnline = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("Notification", JsonSerializer.Serialize(new
            {
                Id = "PresenceUpdate",
                Msg = heartbeat
            }));

            return Ok(new
            {
                errorCode = 0,
                roomInstance
            });
        }



        [HttpPost("goto/room/{room}")]
        public async Task<ActionResult> MatchmakeOLD([FromRoute] string room)
        {
            var auth = HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer "))
                return Unauthorized();

            var token = auth["Bearer ".Length..].Trim();

            long playerId;

            try
            {
                var id = _jwt.VerifyToken(token);
                if (!id.HasValue) return Unauthorized();
                playerId = id.Value;
            }
            catch
            {
                return Unauthorized();
            }

            var heartbeat = await _db.Heartbeats
                .FirstOrDefaultAsync(h => h.PlayerId == playerId);

            if (heartbeat == null)
            {
                heartbeat = new Heartbeat
                {
                    PlayerId = playerId,
                    StatusVisibility = 0,
                    Platform = 0,
                    DeviceClass = 0,
                    VrMovementMode = 0,
                    IsOnline = true,
                    LastOnline = DateTime.UtcNow,
                    AppVersion = "20230406"
                };

                _db.Heartbeats.Add(heartbeat);
                await _db.SaveChangesAsync();
            }

            RoomInstance? instance = null;

            if (heartbeat.RoomInstanceId.HasValue)
            {
                instance = await _db.Set<RoomInstance>()
                    .FirstOrDefaultAsync(r => r.RoomInstanceId == heartbeat.RoomInstanceId.Value);
            }

            if (instance == null)
            {
                var template = await _db.Set<RoomInstance>()
                    .FirstOrDefaultAsync(r => r.RoomId == 1);

                if (template == null)
                {
                    // FIX: was a 500 on fresh databases. Create the template row.
                    // Private so matchmaking never picks it.
                    template = new RoomInstance
                    {
                        Name = "Template",
                        RoomId = 1,
                        SubRoomId = 0,
                        Location = "76d98498-60a1-430c-ab76-b54a29b7a163",
                        RoomInstanceType = 0,
                        PhotonRegionId = "us",
                        PhotonRegion = "us",
                        PhotonRoomId = Guid.NewGuid().ToString(),
                        MaxCapacity = 10,
                        IsFull = false,
                        IsPrivate = true,
                        IsInProgress = false,
                        MatchmakingPolicy = 0
                    };
                    _db.Set<RoomInstance>().Add(template);
                    await _db.SaveChangesAsync();
                }

                instance = new RoomInstance
                {
                    RoomId = template.RoomId,
                    SubRoomId = template.SubRoomId,
                    Location = template.Location,
                    RoomInstanceType = template.RoomInstanceType,
                    PhotonRegionId = template.PhotonRegionId,
                    PhotonRegion = template.PhotonRegion,
                    PhotonRoomId = Guid.NewGuid().ToString(),
                    Name = room,
                    MaxCapacity = template.MaxCapacity,
                    IsFull = false,
                    IsPrivate = false,
                    IsInProgress = false,
                    MatchmakingPolicy = template.MatchmakingPolicy
                };

                _db.Set<RoomInstance>().Add(instance);
                await _db.SaveChangesAsync();

                heartbeat.RoomInstanceId = instance.RoomInstanceId;
                await _db.SaveChangesAsync();
            }

            var playersInRoom = await _db.Heartbeats
                .Where(h => h.RoomInstanceId == instance.RoomInstanceId)
                .CountAsync();

            if (playersInRoom >= instance.MaxCapacity)
            {
                instance.IsFull = true;
                await _db.SaveChangesAsync();
            }

            heartbeat.RoomInstanceId = instance.RoomInstanceId;
            heartbeat.LastOnline = DateTime.UtcNow;
            heartbeat.IsOnline = true;

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("Notification", System.Text.Json.JsonSerializer.Serialize(new
            {
                Id = "PresenceUpdate",
                Msg = new
                {
                    heartbeat.PlayerId,
                    heartbeat.StatusVisibility,
                    heartbeat.Platform,
                    heartbeat.DeviceClass,
                    heartbeat.RoomInstanceId,
                    heartbeat.VrMovementMode,
                    heartbeat.LastOnline,
                    heartbeat.IsOnline,
                    heartbeat.AppVersion
                }
            }));

            return Ok(new
            {
                errorCode = 0,
                roomInstance = new
                {
                    instance.RoomInstanceId,
                    instance.RoomId,
                    instance.SubRoomId,
                    instance.Name,
                    instance.MaxCapacity,
                    instance.IsFull,
                    instance.IsPrivate,
                    instance.IsInProgress,
                    instance.PhotonRoomId
                }
            });
        }

        [HttpPost("room/{roomid}")]
        public async Task<ActionResult> MatchmakeByName([FromRoute] long roomid)
        {
            var token = HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
                return Unauthorized();

            token = token["Bearer ".Length..].Trim();

            long? playerIdNullable;

            try
            {
                playerIdNullable = _jwt.VerifyToken(token);
            }
            catch
            {
                return Unauthorized();
            }

            if (!playerIdNullable.HasValue)
                return Unauthorized();

            long playerId = playerIdNullable.Value;

            var heartbeat = await _db.Heartbeats
                .Include(h => h.RoomInstance)
                .FirstOrDefaultAsync(h => h.PlayerId == playerId);

            if (heartbeat == null)
            {
                heartbeat = new Heartbeat
                {
                    PlayerId = playerId,
                    IsOnline = true,
                    LastOnline = DateTime.UtcNow,
                    StatusVisibility = 0,
                    Platform = 0,
                    DeviceClass = 0,
                    VrMovementMode = 0,
                    AppVersion = "20230406"
                };

                _db.Heartbeats.Add(heartbeat);
            }

            var candidates = await _db.Heartbeats
                .Include(h => h.RoomInstance)
                .Where(h =>
                    h.RoomInstance != null &&
                    h.RoomInstance.RoomId == roomid &&
                    !h.RoomInstance.IsPrivate)
                .ToListAsync();

            var roomInstance = candidates
                .GroupBy(h => h.RoomInstanceId)
                .Select(g => new
                {
                    Room = g.First().RoomInstance,
                    Count = g.Count()
                })
                .FirstOrDefault(x =>
                    x.Room != null &&
                    x.Count < x.Room.MaxCapacity)?
                .Room;

            if (roomInstance == null)
            {
                roomInstance = new RoomInstance
                {
                    Name = "A Room.",
                    RoomId = roomid,
                    SubRoomId = 0,
                    Location = "76d98498-60a1-430c-ab76-b54a29b7a163",
                    RoomInstanceType = 0,
                    PhotonRegionId = "us",
                    PhotonRegion = "us",
                    PhotonRoomId = Guid.NewGuid().ToString(),
                    MaxCapacity = 10,
                    IsFull = false,
                    IsPrivate = false,
                    IsInProgress = false,
                    MatchmakingPolicy = 0
                };
                // FIX: add explicitly instead of relying on EF cascade from heartbeat.
                _db.Set<RoomInstance>().Add(roomInstance);
            }

            heartbeat.RoomInstance = roomInstance;
            heartbeat.RoomInstanceId = roomInstance.RoomInstanceId;
            heartbeat.IsOnline = true;
            heartbeat.LastOnline = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("Notification", JsonSerializer.Serialize(new
            {
                Id = "PresenceUpdate",
                Msg = heartbeat
            }));

            return Ok(new
            {
                errorCode = 0,
                roomInstance
            });
        }
    }
}