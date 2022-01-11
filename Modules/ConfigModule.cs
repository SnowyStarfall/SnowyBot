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
      IUserMessage m = await Context.Channel.SendMessageAsync("Not yet implemented.").ConfigureAwait(false);
      await Task.Delay(5000).ConfigureAwait(false);
      await m.DeleteAsync().ConfigureAwait(false);
    }
    [Command("Prefix")]
    public async Task Prefix([Remainder] string input = null)
    {
      SocketGuildUser user = Context.User as SocketGuildUser;
      if (input == null || input.Length == 0)
      {
        await ReplyAsync($"Server prefix is `{await guilds.GetGuildPrefix(Context.Guild.Id).ConfigureAwait(false)}`.").ConfigureAwait(false);
        return;
      }
      if(!user.GuildPermissions.Administrator)
      {
        IUserMessage m = await ReplyAsync($"You lack the permissions to use this command.").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m.DeleteAsync().ConfigureAwait(false);
        return;
      }
      if (input.Length > 8)
      {
        IUserMessage m = await ReplyAsync("Prefix too long!").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m.DeleteAsync().ConfigureAwait(false);
        return;
      }
      await guilds.ModifyGuildPrefix(Context.Guild.Id, input).ConfigureAwait(false);
      IUserMessage m1 = await ReplyAsync($"Server prefix set to `{input}`.").ConfigureAwait(false);
      await Task.Delay(5000).ConfigureAwait(false);
      await m1.DeleteAsync().ConfigureAwait(false);
      return;
    }
    [Command("DeleteMusic")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteMusic()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      await guilds.DeleteMusic(Context.Guild.Id, Context).ConfigureAwait(false);
      IUserMessage m = await Context.Channel.SendMessageAsync($"Deletion of music posts {(guild.DeleteMusic ? "enabled" : "disabled")}/").ConfigureAwait(false);
      await Task.Delay(5000).ConfigureAwait(false);
      await m.DeleteAsync().ConfigureAwait(false);
    }
    [Command("Welcome")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Welcome([Remainder] string message = null)
    {
      if (message?.Length == 0)
      {
        await guilds.CreateWelcomeMessage(Context.Guild.Id, 0, message);
        IUserMessage m1 = await Context.Channel.SendMessageAsync("Welcome message disabled.").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m1.DeleteAsync().ConfigureAwait(false);
        return;
      }

      IMessageChannel channel = null;
      IUserMessage m2 = await Context.Channel.SendMessageAsync("Mention the channel you would like the welcome to appear in.").ConfigureAwait(false);

      var result1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (result1.IsSuccess)
      {
        await m2.DeleteAsync().ConfigureAwait(false);
        if (!TryParseChannel(result1.Value.Content, out ulong channelID1))
        {
          IUserMessage m3 = await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
          return;
        }
        channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
      }
      else
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m4.DeleteAsync().ConfigureAwait(false);
        return;
      }

      await guilds.CreateWelcomeMessage(Context.Guild.Id, channel.Id, message).ConfigureAwait(false);
      IUserMessage m5 = await Context.Channel.SendMessageAsync($"Welcome message set to\n>>> {message}").ConfigureAwait(false);
      await Task.Delay(5000).ConfigureAwait(false);
      await m5.DeleteAsync().ConfigureAwait(false);
    }
    [Command("Goodbye")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Goodbye([Remainder] string message = null)
    {
      if (message?.Length == 0)
      {
        await guilds.CreateGoodbyeMessage(Context.Guild.Id, 0, message);
        IUserMessage m1 = await Context.Channel.SendMessageAsync("Goodbye message disabled.").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m1.DeleteAsync().ConfigureAwait(false);
        return;
      }

      IMessageChannel channel = null;
      IUserMessage m2 = await Context.Channel.SendMessageAsync("Mention the channel you would like the goodbye to appear in.").ConfigureAwait(false);

      var result1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (result1.IsSuccess)
      {
        await m2.DeleteAsync().ConfigureAwait(false);
        if (!TryParseChannel(result1.Value.Content, out ulong channelID1))
        {
          IUserMessage m3 = await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
          return;
        }
        channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
      }
      else
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await m4.DeleteAsync().ConfigureAwait(false);
        return;
      }

      IUserMessage m5 = await Context.Channel.SendMessageAsync($"Goodbye message set to\n>>> {message}").ConfigureAwait(false);
      await Task.Delay(5000).ConfigureAwait(false);
      await m5.DeleteAsync().ConfigureAwait(false);
      await guilds.CreateGoodbyeMessage(Context.Guild.Id, channel.Id, message).ConfigureAwait(false);
    }
  }
}
