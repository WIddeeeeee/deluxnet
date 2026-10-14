using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeluxeNET.Migrations
{
    /// <inheritdoc />
    public partial class repAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reputations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountId = table.Column<long>(type: "bigint", nullable: false),
                    IsCheerful = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Noteriety = table.Column<float>(type: "float", nullable: false),
                    SelectedCheer = table.Column<int>(type: "int", nullable: false),
                    CheerCredit = table.Column<int>(type: "int", nullable: false),
                    CheerGeneral = table.Column<int>(type: "int", nullable: false),
                    CheerHelpful = table.Column<int>(type: "int", nullable: false),
                    CheerCreative = table.Column<int>(type: "int", nullable: false),
                    CheerGreatHost = table.Column<int>(type: "int", nullable: false),
                    CheerSportsman = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reputations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reputations");
        }
    }
}
