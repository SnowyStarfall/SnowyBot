using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using SnowyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
  public class CommandHandler
  {
    public static IServiceProvider provider;
    public static DiscordSocketClient discord;
    public static CommandService commands;
    public static Guilds guilds;
    public int playerCount;
    public CommandHandler(DiscordSocketClient _discord, CommandService _commands, IServiceProvider _provider, Guilds _guilds)
    {
      provider = _provider;
      discord = _discord;
      commands = _commands;
      guilds = _guilds;

      discord.MessageReceived += OnMessageRecieved;
    }
    private async Task OnMessageRecieved(SocketMessage arg)
    {
      var socketMessage = arg as SocketUserMessage;

      if (!(arg is SocketUserMessage message) || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
        return;

      var context = new SocketCommandContext(discord, socketMessage);
      var prefix = await guilds.GetGuildPrefix(context.Guild.Id).ConfigureAwait(false) ?? "!";
      var argPos = 0;

      if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(DiscordService.client.CurrentUser, ref argPos))
        return;

      var blacklistedChannelCheck = from a in GlobalData.Config.BlacklistedChannels
                                    where a == context.Channel.Id
                                    select a;
      var blacklistedChannel = blacklistedChannelCheck.FirstOrDefault();

      if (blacklistedChannel != context.Channel.Id)
      {
        await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best).ConfigureAwait(false);
        return;
      }
    }
  }
}
