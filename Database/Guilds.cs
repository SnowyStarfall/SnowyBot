using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using SnowyBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Database
{
  public class Guilds
  {
    private readonly GuildContext context;

    public Guilds(GuildContext _context)
    {
      context = _context;
    }
    public async Task<Guild> GetGuild(ulong id)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = "!" });
      return guild;
    }
    public async Task<string> GetGuildPrefix(ulong id)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
      {
        context.Add(new Guild { ID = id, Prefix = "!" });
      }
      else if (guild.Prefix?.Length == 0)
      {
        guild.Prefix = "!";
        await context.SaveChangesAsync().ConfigureAwait(false);
      }

      var prefix = await context.Guilds
        .AsAsyncEnumerable()
        .Where(x => x.ID == id)
        .Select(x => x.Prefix)
        .FirstOrDefaultAsync()
        .ConfigureAwait(false);

      return await Task.FromResult(prefix).ConfigureAwait(false);
    }
    public async Task ModifyGuildPrefix(ulong id, string prefix)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = prefix });
      else
        guild.Prefix = prefix;

      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task AddReactiveRole(ulong guildID, ulong channelID, ulong messageID, ulong roleID, string emote)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
      {
        context.Add(new Guild { ID = guildID, Prefix = "!", Roles = $"{channelID};{messageID};{roleID};{emote}" });
        return;
      }
      if (guild.Roles == string.Empty || guild.Roles == null)
        guild.Roles = $"{channelID};{messageID};{roleID};{emote}";
      else
        guild.Roles += $"|{channelID};{messageID};{roleID};{emote}";
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task RemoveReactiveRole(ulong guildID, ulong channelID, ulong messageID, ulong roleID, string emote)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = guildID, Prefix = "!"});
      if (!guild.Roles.Contains($"{channelID};{messageID};{roleID};{emote}"))
        return;
      int index = guild.Roles.IndexOf($"{channelID};{messageID};{roleID};{emote}");
      if (index == 0)
        guild.Roles = guild.Roles.Replace($"{channelID};{messageID};{roleID};{emote}", "");
      else
        guild.Roles = guild.Roles.Replace($"|{channelID};{messageID};{roleID};{emote}", "");
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task<string> ExistsReactiveRole(ulong guildID, ulong messageID, string emote)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
      {
        context.Add(new Guild { ID = guildID, Prefix = "!" });
        return null;
      }
      string[] split = guild.Roles.Split('|');
      foreach(string s in split)
      {
        if (s.Contains(messageID.ToString()) && s.Contains(emote))
        {
          string[] split2 = s.Split(";");
          return split2[2];
        }
      }
      return null;
    }
    public async Task DeleteMusic(ulong id, ICommandContext commandContext)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = "!" });
      guild.DeleteMusic = !guild.DeleteMusic;
      string response = guild.DeleteMusic ? "Music posts will be deleted." : "Music posts will not be deleted.";
      await commandContext.Channel.SendMessageAsync(response).ConfigureAwait(false);
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task CreateWelcomeMessage(ulong id, ulong channelID, string message)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = "!" });
      guild.WelcomeMessage = channelID.ToString() + ";" + message;
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task<string> GetWelcomeMessage(ulong id)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      return guild.WelcomeMessage;
    }
    public async Task CreateGoodbyeMessage(ulong id, ulong channelID, string message)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = "!" });
      guild.GoodbyeMessage = channelID.ToString() + ";" + message;
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task<string> GetGoodbyeMessage(ulong id)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      return guild.GoodbyeMessage;
    }
  }
}
