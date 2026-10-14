using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeNET.Migrations
{
    /// <inheritdoc />
    public partial class roomsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "accountId",
                table: "Settings",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Accessibility = table.Column<int>(type: "int", nullable: false),
                    CloningAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorAccountId = table.Column<int>(type: "int", nullable: false),
                    CustomWarning = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisableMicAutoMute = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisableRoomComments = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EncryptVoiceChat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImageName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeveloperOwned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDorm = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRRO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LoadScreenLocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxPlayerCalculationMode = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    MinLevel = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<int>(type: "int", nullable: false),
                    SupportsJuniors = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsLevelVoting = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsMobile = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsQuest2 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsScreens = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsTeleportVR = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsVRLow = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportsWalkVR = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Tags = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WarningMask = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InvitedRole = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomRoles_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomStats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    CheerCount = table.Column<int>(type: "int", nullable: false),
                    FavoriteCount = table.Column<int>(type: "int", nullable: false),
                    VisitCount = table.Column<int>(type: "int", nullable: false),
                    VisitorCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomStats_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubRooms",
                columns: table => new
                {
                    SubRoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    Accessibility = table.Column<int>(type: "int", nullable: false),
                    IsSandbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitySceneId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubRooms", x => x.SubRoomId);
                    table.ForeignKey(
                        name: "FK_SubRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubRoomSaves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    SubRoomId = table.Column<int>(type: "int", nullable: false),
                    SubRoomDataSaveId = table.Column<int>(type: "int", nullable: false),
                    DataBlob = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SavedByAccountId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UnityAssetId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubRoomId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubRoomSaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubRoomSaves_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubRoomSaves_SubRooms_SubRoomId",
                        column: x => x.SubRoomId,
                        principalTable: "SubRooms",
                        principalColumn: "SubRoomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubRoomSaves_SubRooms_SubRoomId1",
                        column: x => x.SubRoomId1,
                        principalTable: "SubRooms",
                        principalColumn: "SubRoomId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RoomRoles_RoomId",
                table: "RoomRoles",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomStats_RoomId",
                table: "RoomStats",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubRooms_RoomId",
                table: "SubRooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_SubRoomSaves_RoomId",
                table: "SubRoomSaves",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_SubRoomSaves_SubRoomId",
                table: "SubRoomSaves",
                column: "SubRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubRoomSaves_SubRoomId1",
                table: "SubRoomSaves",
                column: "SubRoomId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomRoles");

            migrationBuilder.DropTable(
                name: "RoomStats");

            migrationBuilder.DropTable(
                name: "SubRoomSaves");

            migrationBuilder.DropTable(
                name: "SubRooms");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.AlterColumn<long>(
                name: "accountId",
                table: "Settings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
