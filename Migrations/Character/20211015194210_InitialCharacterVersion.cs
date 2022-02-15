using Microsoft.EntityFrameworkCore.Migrations;

namespace SnowyBot.Migrations.Character
{
	public partial class InitialCharacterVersion : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterDatabase()
					.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateTable(
					name: "Characters",
					columns: table => new {
						CharacterID = table.Column<string>(type: "varchar(95)", nullable: false)
									.Annotation("MySql:CharSet", "utf8mb4"),
						UserID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
						CreationDate = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Prefix = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Name = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Gender = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Sex = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Species = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Age = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Height = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Weight = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Orientation = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						Description = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						AvatarURL = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4"),
						ReferenceURL = table.Column<string>(type: "longtext", nullable: true)
									.Annotation("MySql:CharSet", "utf8mb4")
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Characters", x => x.CharacterID);
					})
					.Annotation("MySql:CharSet", "utf8mb4");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "Characters");
		}
	}
}
