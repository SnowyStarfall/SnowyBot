using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnowyBot.Database
{
  public class GuildContext : DbContext
  {
    public DbSet<Guild> Guilds { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
      => options.UseMySql("server=localhost;user=root;database=snowybot_guilds;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(0, 0, 0, 1)));
  }
  public class Guild
  {
    public ulong ID { get; set; }
    public string Prefix { get; set; }
    public ulong RoleChannel { get; set; }
    public ulong RoleMessage { get; set; }
  }
}
