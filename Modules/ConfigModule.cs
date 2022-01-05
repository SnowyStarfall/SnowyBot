using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using System.Threading.Tasks;
using System;
using SnowyBot.Services;
using static Discord.MentionUtils;

namespace SnowyBot.Modules
{
  public class ConfigModule : ModuleBase
  {
    public readonly Guilds guilds;
    public ConfigModule(Guilds _guilds) => guilds = _guilds;

    [Command("Config")]
    public async Task Config()
    {

    }
    [Command("Prefix")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Prefix([Remainder] string query = null)
    {
      SocketGuildUser user = Context.User as SocketGuildUser;
      string[] input = query?.Split(" ");
      //if (!user.GuildPermissions.Administrator && ((input?.Length == 2 && input[0] == "set") || (input?.Length == 1 && input[0] == "remove")))
      //{
      //  await ReplyAsync("You do not have the permissions for this command.").ConfigureAwait(false);
      //  return;
      //}
      if (input?.Length == 2 && input[0] == "set")
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
      else if (input?.Length == 1 && input[0] == "remove")
      {
        await guilds.ModifyGuildPrefix(Context.Guild.Id, "!").ConfigureAwait(false);
        await ReplyAsync("Server prefix is now `!`.").ConfigureAwait(false);
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
    [Command("DeleteMusic")]
    public async Task DeleteMusic()
    {
      await guilds.DeleteMusic(Context.Guild.Id, Context).ConfigureAwait(false);
    }
    [Command("Welcome")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Welcome([Remainder] string message = null)
    {
      if (message?.Length == 0)
      {
        await Context.Channel.SendMessageAsync("Welcome message disabled.").ConfigureAwait(false);
        await guilds.CreateWelcomeMessage(Context.Guild.Id, 0, message);
        return;
      }

      IMessageChannel channel = null;
      await Context.Channel.SendMessageAsync("Mention the channel you would like the welcome to appear in.").ConfigureAwait(false);

      var result1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (result1.IsSuccess)
      {
        if (!TryParseChannel(result1.Value.Content, out ulong channelID1))
        {
          await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
          return;
        }
        channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync($"Welcome message set to\n>>> {message}").ConfigureAwait(false);
      await guilds.CreateWelcomeMessage(Context.Guild.Id, channel.Id, message).ConfigureAwait(false);
    }
    [Command("Goodbye")]
    public async Task Goodbye([Remainder] string message = null)
    {
      if (message?.Length == 0)
      {
        await Context.Channel.SendMessageAsync("Goodbye message disabled.").ConfigureAwait(false);
        await guilds.CreateGoodbyeMessage(Context.Guild.Id, 0, message);
        return;
      }

      IMessageChannel channel = null;
      await Context.Channel.SendMessageAsync("Mention the channel you would like the goodbye to appear in.").ConfigureAwait(false);

      var result1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (result1.IsSuccess)
      {
        if (!TryParseChannel(result1.Value.Content, out ulong channelID1))
        {
          await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
          return;
        }
        channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync($"Goodbye message set to\n>>> {message}").ConfigureAwait(false);
      await guilds.CreateGoodbyeMessage(Context.Guild.Id, channel.Id, message).ConfigureAwait(false);
    }
    //[Command("Roles")]
    //public async Task Roles()
    //{
    //  await ReplyAsync("Not yet implemented.").ConfigureAwait(false);
    //}
  }
}
