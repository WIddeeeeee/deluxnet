using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeNET.Migrations
{
    /// <inheritdoc />
    public partial class FixedHeartbeatAndRoomInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Heartbeats_RoomInstance_RoomInstanceId",
                table: "Heartbeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomInstance",
                table: "RoomInstance");

            migrationBuilder.DropColumn(
                name: "IsRoomInstanceNull",
                table: "Heartbeats");

            migrationBuilder.RenameTable(
                name: "RoomInstance",
                newName: "RoomInstances");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RoomInstances",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomInstances",
                table: "RoomInstances",
                column: "RoomInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInstances_Name",
                table: "RoomInstances",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Heartbeats_RoomInstances_RoomInstanceId",
                table: "Heartbeats",
                column: "RoomInstanceId",
                principalTable: "RoomInstances",
                principalColumn: "RoomInstanceId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Heartbeats_RoomInstances_RoomInstanceId",
                table: "Heartbeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomInstances",
                table: "RoomInstances");

            migrationBuilder.DropIndex(
                name: "IX_RoomInstances_Name",
                table: "RoomInstances");

            migrationBuilder.RenameTable(
                name: "RoomInstances",
                newName: "RoomInstance");

            migrationBuilder.AddColumn<bool>(
                name: "IsRoomInstanceNull",
                table: "Heartbeats",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RoomInstance",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomInstance",
                table: "RoomInstance",
                column: "RoomInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Heartbeats_RoomInstance_RoomInstanceId",
                table: "Heartbeats",
                column: "RoomInstanceId",
                principalTable: "RoomInstance",
                principalColumn: "RoomInstanceId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
