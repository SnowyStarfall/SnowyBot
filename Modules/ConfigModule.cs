using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
  public class ConfigModule : ModuleBase
  {
    public readonly Guilds guilds;
    public ConfigModule(Guilds _guilds) => guilds = _guilds;

    [Command("Prefix")]
    public async Task Prefix([Remainder] string query = null)
    {
      SocketGuildUser user = Context.User as SocketGuildUser;
      string[] input = query?.Split(" ");
      if (input?.Length == 2 && input[0] == "set" && user.GuildPermissions.Administrator)
      {
        if (input[1].Length > 8)
        {
          await ReplyAsync("Prefix too long!").ConfigureAwait(false);
          return;
        }
        await guilds.ModifyGuildPrefix(Context.Guild.Id, input[1]).ConfigureAwait(false);
        await ReplyAsync($"Server prefix set to `{input[1]}`.").ConfigureAwait(false);
        return;
      }
      else if (input?.Length == 1 && input[0] == "remove" && user.GuildPermissions.Administrator)
      {
        await guilds.ModifyGuildPrefix(Context.Guild.Id, "!").ConfigureAwait(false);
        await ReplyAsync("Server prefix is now `!`.").ConfigureAwait(false);
        return;
      }
      else if (!user.GuildPermissions.Administrator && ((input?.Length == 2 && input[0] == "set") || (input?.Length == 1 && input[0] == "remove")))
      {
        await ReplyAsync("You do not have the permissions for this command.").ConfigureAwait(false);
        return;
      }
      else if (input == null)
      {
        await ReplyAsync($"Your server prefix is `{guilds.GetGuildPrefix(Context.Guild.Id).Result}`.").ConfigureAwait(false);
        return;
      }
      else
      {
        await Context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Prefix", "Please specify all parts of the command.", new Color(0xac4554))).ConfigureAwait(false);
        return;
      }
    }
    [Command("Roles")]
    public async Task Roles()
    {
      await ReplyAsync("Not yet implemented.").ConfigureAwait(false);
    }
  }
}
