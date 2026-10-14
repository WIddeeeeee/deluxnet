using DeluxeNET.Data;
using DeluxeNET.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class roomserverController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly jwt _jwt;

        public roomserverController(AppDbContext db, jwt jwt)
        {
            _jwt = jwt;
            _db = db;
        }

        [HttpGet("rooms")]
        public async Task<IActionResult> getRoomDetails([FromQuery] string name)
        {
            //var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            //if (accountId == null) return Unauthorized();

            var room = await _db.Rooms
                .Include(x => x.Stats)
                .Include(x => x.Roles)
                .Include(x => x.SubRooms)
                .FirstOrDefaultAsync(x => x.Name == name);

            if (room == null) return NotFound();

            room.PromoImages = new List<object>();
            room.PromoExternalContent = new List<object>();
            room.LoadScreens = new List<object>();

            return Ok(room);
        }

        [HttpGet("rooms/{roomid}")]
        public async Task<IActionResult> getRoomDetailsByID([FromRoute] int roomid)
        {
            //var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            //if (accountId == null) return Unauthorized();

            var isZero = false;

            if (roomid == 0)
            {
                roomid = 2;

                isZero = true;
            }

            var room = await _db.Rooms
                .Include(x => x.Stats)
                .Include(x => x.Roles)
                .Include(x => x.SubRooms)
                .FirstOrDefaultAsync(x => x.RoomId == roomid);

            if (room == null) return NotFound();

            if (isZero)
            {
                room.RoomId = 0;
            }

            room.PromoImages = new List<object>();
            room.PromoExternalContent = new List<object>();
            room.LoadScreens = new List<object>();

            return Ok(room);
        }

        [HttpGet("rooms/bulk")]
        public async Task<IActionResult> getRoomDetailsBulkByRoomNames([FromQuery] List<string> name)
        {
            //var accountId = _jwt.VerifyToken(Request.Headers["Authorization"].ToString());

            //if (accountId == null) return Unauthorized();


            var room = await _db.Rooms
                .Include(x => x.Stats)
                .Include(x => x.Roles)
                .Include(x => x.SubRooms)
                .Where(x => name.Contains(x.Name))
                .ToListAsync();

            if (room == null) return NotFound();

            foreach (var i in room)
            {
                i.PromoImages = new List<object>();
                i.PromoExternalContent = new List<object>();
                i.LoadScreens = new List<object>();
            }

            return Ok(room);
        }

        [HttpGet("rooms/createdby/me")]
        public IActionResult getMyRoomsIMade()
        {
            return Ok(new List<object>());
        }
        [HttpGet("rooms/ownedby/{playerid}")]
        public IActionResult getRoomsOwnedByID()
        {
            return Ok(new
            {
                Results = new List<object>(),
                TotalResults = 0
            });
        }

        [HttpGet("rooms/visitedby/me")]
        public IActionResult visitroomsbyme()
        {
            return Ok(new
            {
                Results = new List<object>(),
                TotalResults = 0
            });
        }

        [HttpGet("rooms/hot")]
        public async Task<IActionResult> getRoomsthatAreHot([FromQuery] List<string> tag,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100
            )
        {

            if (take > 500) take = 100;

            var room = await _db.Rooms
                .Include(x => x.Stats)
                .Include(x => x.Roles)
                .Include(x => x.SubRooms)
                .Skip(skip)
                .Take(take)
                .ToListAsync();


            foreach (var i in room)
            {
                i.PromoImages = new List<object>();
                i.PromoExternalContent = new List<object>();
                i.LoadScreens = new List<object>();
            }

            return Ok(new
            {
                Results = room,
                TotalResults = room.Count()
            });
        }
    }
}
