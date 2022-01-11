using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowyBot.Migrations
{
    public partial class FourthVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPoint",
                table: "Guilds",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PointGain",
                table: "Guilds",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserPoint",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "PointGain",
                table: "Guilds");
        }
    }
}
