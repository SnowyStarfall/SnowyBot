using Microsoft.EntityFrameworkCore.Migrations;

namespace SnowyBot.Migrations
{
	public partial class SecondVersion : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<ulong>(
					name: "RoleChannel",
					table: "Guilds",
					type: "bigint unsigned",
					nullable: false,
					defaultValue: 0ul);

			migrationBuilder.AddColumn<ulong>(
					name: "RoleMessage",
					table: "Guilds",
					type: "bigint unsigned",
					nullable: false,
					defaultValue: 0ul);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "RoleChannel",
					table: "Guilds");

			migrationBuilder.DropColumn(
					name: "RoleMessage",
					table: "Guilds");
		}
	}
}
