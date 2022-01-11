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
    // Reactive Roles string for the Guild
    // channel;message;role;emoji|
    public string Roles { get; set; }
    // If the bot should delete music posts after posting them
    public bool DeleteMusic { get; set; }
    // If the bot should greet new guild members. Empty means no.
    public string WelcomeMessage { get; set; }
    // If the bot should say goodbye to members. Empty means no.
    public string GoodbyeMessage { get; set; }
    // Stores XP data
    // userID;XP|
    public string UserPoints { get ; set; }
    // Stores the integer range of which XP is gained
    // minimum;maximum
    public string PointGain { get; set; }
    // Stores the channel ID for the update channel, if empty, will not post.
    public ulong ChangelogID { get; set; }
  }
}
