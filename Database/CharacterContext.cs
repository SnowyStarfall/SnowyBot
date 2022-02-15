using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnowyBot.Database
{
	public class CharacterContext : DbContext
	{
		public DbSet<Character> Characters { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseMySql("server=localhost;user=root;database=snowybot_characters;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(0, 0, 0, 1)));
	}
	public class Character
	{
		public string CharacterID { get; set; }
		public ulong UserID { get; set; }
		public string CreationDate { get; set; }
		public string Prefix { get; set; }
		public string Name { get; set; }
		public string Gender { get; set; }
		public string Sex { get; set; }
		public string Species { get; set; }
		public string Age { get; set; }
		public string Height { get; set; }
		public string Weight { get; set; }
		public string Orientation { get; set; }
		public string Description { get; set; }
		public string AvatarURL { get; set; }
		public string ReferenceURL { get; set; }
	}
}
