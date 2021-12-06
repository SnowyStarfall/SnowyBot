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
    private readonly DiscordSocketClient client;
    private readonly CommandService commands;
    private readonly Guilds guilds;

    public StartupService(IServiceProvider _provider, DiscordSocketClient _client, CommandService _commands, Guilds _guilds)
    {
      provider = _provider;
      client = _client;
      commands = _commands;
      guilds = _guilds;
    }

    public async Task StartAsync()
    {
      await client.LoginAsync(TokenType.Bot, DiscordService.config.DiscordToken).ConfigureAwait(false);
      await client.StartAsync().ConfigureAwait(false);

      await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider).ConfigureAwait(false);
    }
  }
}
