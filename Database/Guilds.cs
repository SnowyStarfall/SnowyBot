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
    public async Task AddReactiveMessage(ulong guildID, ulong channelID, ulong messageID)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = guildID, Prefix = "!", Roles = $"{channelID},{messageID};|" });
      if (guild.Roles.Contains($"{channelID},{messageID}"))
        return;
      guild.Roles += $"{channelID},{messageID};|";
    }
    public async Task<bool> ExistsReactiveMessage(ulong guildID, ulong channelID, ulong messageID)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
      {
        context.Add(new Guild { ID = guildID, Prefix = "!", Roles = "" });
        return false;
      }
      return guild.Roles.Contains($"{channelID},{messageID}");
    }
    public async Task RemoveReactiveMessage(ulong guildID, ulong channelID, ulong messageID)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = guildID, Prefix = "!", Roles = $"" });
      if (!guild.Roles.Contains($"{channelID},{messageID}"))
        return;
      int index1 = guild.Roles.IndexOf($"{channelID},{messageID}");
      int index2 = guild.Roles.IndexOf("|", index1);
      guild.Roles = guild.Roles.Remove(index1, index2 - index1);
      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task AddReactiveRole(ulong guildID, ulong channelID, ulong messageID, ulong roleID, ulong emoteID)
    {
      var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = guildID, Prefix = "!", Roles = $"{channelID},{messageID};{roleID},{emoteID}|" });
      if (!guild.Roles.Contains($"{channelID},{messageID}"))
        return;
      int index1 = guild.Roles.IndexOf($"{channelID},{messageID}");
      int index2 = guild.Roles.IndexOf("|", index1);
      int index3 = guild.Roles.IndexOf($"{roleID},{emoteID}", index1);
      if (index3 == -1 || index3 > index2)
        guild.Roles = guild.Roles.Insert(index2 - 1, $"{roleID},{emoteID}");


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
