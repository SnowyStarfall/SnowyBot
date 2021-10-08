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

    public async Task ModifyGuildPrefix(ulong id, string prefix)
    {
      var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
      if (guild == null)
        context.Add(new Guild { ID = id, Prefix = prefix });
      else
        guild.Prefix = prefix;

      await context.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task<string> GetGuildPrefix(ulong id)
    {
      //var guild = context.Guilds.Find(id);
      //if (guild == null)
      //{
      //  context.Add(new Guild { ID = id, Prefix = "!" });
      //}
      //else if (guild.Prefix?.Length == 0)
      //{
      //  guild.Prefix = "!";
      //  await context.SaveChangesAsync().ConfigureAwait(false);
      //}

      var prefix = await context.Guilds
        .AsAsyncEnumerable()
        .Where(x => x.ID == id)
        .Select(x => x.Prefix)
        .FirstOrDefaultAsync()
        .ConfigureAwait(false);

      return await Task.FromResult(prefix).ConfigureAwait(false);
    }
    public async Task ModifyRoleMessage(ulong id, string text, CommandContext con)
    {
      var guild = DiscordService.client.GetGuild(id);

      var channelID = await context.Guilds.AsAsyncEnumerable()
                             .Where(x => x.ID == id)
                             .Select(x => x.RoleChannel)
                             .FirstOrDefaultAsync()
                             .ConfigureAwait(false);
      var messageID = await context.Guilds.AsAsyncEnumerable()
                             .Where(x => x.ID == id)
                             .Select(x => x.RoleMessage)
                             .FirstOrDefaultAsync()
                             .ConfigureAwait(false);

      var channel = con.Guild.GetChannelAsync(channelID) as IMessageChannel;
      var message = channel.GetMessageAsync(messageID) as IUserMessage;

      await message.ModifyAsync((MessageProperties properties) => properties.Content = text).ConfigureAwait(false);
    }
    public async Task CreateRoleMessage(ulong id)
    {

    }
  }
}
