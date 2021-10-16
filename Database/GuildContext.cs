using Microsoft.EntityFrameworkCore;
using System;
using Discord.Rest;
using System.Collections.Generic;
using Discord;

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
    // ID of the Guild
    public ulong ID { get; set; }
    // Command prefix for the Guild
    public string Prefix { get; set; }
    //// Contains the channel ID's for the Role Messages
    //public string RoleChannel { get; set; }
    //// Contains the message ID's for the Role Messages
    //public string RoleMessage { get; set; }
    //// Contains the role-emoji pairs for the React Roles
    //public string Roles { get; set; }
  }
}
