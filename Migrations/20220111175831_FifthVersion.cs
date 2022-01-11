using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowyBot.Migrations
{
    public partial class FifthVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserPoints",
                table: "Guilds",
                newName: "UserPoints");

            migrationBuilder.AddColumn<ulong>(
                name: "ChangelogID",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangelogID",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "UserPoints",
                table: "Guilds",
                newName: "UserPoints");
        }
    }
}
