using Microsoft.EntityFrameworkCore;

namespace DeluxeNET.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CachedLogin> CachedLogins { get; set; }
        public DbSet<account> Accounts { get; set; }
        public DbSet<setting> Settings { get; set; }
        public DbSet<avatar> Avatars { get; set; }
        public DbSet<progression> Progressions { get; set; }
        public DbSet<reputation> Reputations { get; set; }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomRole> RoomRoles { get; set; }
        public DbSet<RoomStats> RoomStats { get; set; }
        public DbSet<SubRoom> SubRooms { get; set; }
        public DbSet<SubRoomSave> SubRoomSaves { get; set; }

        public DbSet<Heartbeat> Heartbeats { get; set; }
        public DbSet<RoomInstance> RoomInstances { get; set; }

        public DbSet<VolunteerServer> VolunteerServers { get; set; }

        public DbSet<PlayerPage> PlayerPages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>()
                .HasMany(r => r.SubRooms)
                .WithOne(sr => sr.Room)
                .HasForeignKey(sr => sr.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Room>()
                .HasMany(r => r.Roles)
                .WithOne(rr => rr.Room)
                .HasForeignKey(rr => rr.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Stats)
                .WithOne(rs => rs.Room)
                .HasForeignKey<RoomStats>(rs => rs.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubRoom>()
                .HasOne(sr => sr.CurrentSave)
                .WithOne(srs => srs.SubRoom)
                .HasForeignKey<SubRoomSave>(srs => srs.SubRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubRoomSave>()
                .HasOne(srs => srs.Room)
                .WithMany()
                .HasForeignKey(srs => srs.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Heartbeat>()
                .HasOne(h => h.RoomInstance)
                .WithMany(r => r.Heartbeats)
                .HasForeignKey(h => h.RoomInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RoomInstance>()
                .HasIndex(r => r.Name);
        }
    }
}