using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Services
{
  public class StartupService
  {
    public static IServiceProvider provider;
    private readonly DiscordSocketClient discord;
    private readonly CommandService commands;
    private readonly Guilds guilds;

    public StartupService(IServiceProvider _provider, DiscordSocketClient _discord, CommandService _commands, Guilds _guilds)
    {
      provider = _provider;
      discord = _discord;
      commands = _commands;
      guilds = _guilds;

      //DiscordService.client.ReactionAdded += ReactionAdded;
      //DiscordService.client.ReactionRemoved += ReactionRemoved;
    }

    //private Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    //{
    //  if (!guilds.IsRoleMessage(arg1.Id).Result)
    //  {
    //    return Task.CompletedTask;
    //  }
    //  else
    //  {

    //  }
    //}
    //private Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    //{
    //}
    public async Task StartAsync()
    {
      await discord.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken).ConfigureAwait(false);
      await discord.StartAsync().ConfigureAwait(false);

      await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider).ConfigureAwait(false);
    }
  }
}
