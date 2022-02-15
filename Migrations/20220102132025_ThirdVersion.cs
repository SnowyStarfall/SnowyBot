using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowyBot.Migrations
{
	public partial class ThirdVersion : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
					name: "DeleteMusic",
					table: "Guilds",
					type: "tinyint(1)",
					nullable: false,
					defaultValue: false);

			migrationBuilder.AddColumn<string>(
					name: "GoodbyeMessage",
					table: "Guilds",
					type: "longtext",
					nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AddColumn<string>(
					name: "Roles",
					table: "Guilds",
					type: "longtext",
					nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AddColumn<string>(
					name: "WelcomeMessage",
					table: "Guilds",
					type: "longtext",
					nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "DeleteMusic",
					table: "Guilds");

			migrationBuilder.DropColumn(
					name: "GoodbyeMessage",
					table: "Guilds");

			migrationBuilder.DropColumn(
					name: "Roles",
					table: "Guilds");

			migrationBuilder.DropColumn(
					name: "WelcomeMessage",
					table: "Guilds");
		}
	}
}
