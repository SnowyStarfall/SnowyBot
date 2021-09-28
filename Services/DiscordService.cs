using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SnowyBot.Services;
using SnowyBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;

namespace SnowyBot.Services
{
  public class DiscordService
  {
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _commandHandler;
    private readonly ServiceProvider _services;
    private readonly LavaNode _lavaNode;
    private readonly LavaLinkAudio _audioService;
    private readonly GlobalData _globalData;

    public DiscordService()
    {
      _services = ConfigureServices();
      _client = _services.GetRequiredService<DiscordSocketClient>();
      _commandHandler = _services.GetRequiredService<CommandHandler>();
      _lavaNode = _services.GetRequiredService<LavaNode>();
      _globalData = _services.GetRequiredService<GlobalData>();
      _audioService = _services.GetRequiredService<LavaLinkAudio>();

      SubscribeLavaLinkEvents();
      SubscribeDiscordEvents();
    }

    public async Task InitializeAsync()
    {
      await InitializeGlobalDataAsync().ConfigureAwait(false);

      await _client.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken).ConfigureAwait(false);
      await _client.StartAsync().ConfigureAwait(false);

      await _commandHandler.InitializeAsync().ConfigureAwait(false);

      await Task.Delay(-1).ConfigureAwait(false);
    }

    private void SubscribeLavaLinkEvents()
    {
      _lavaNode.OnLog += LogAsync;
      _lavaNode.OnTrackEnded += _audioService.TrackEnded;
    }

    private void SubscribeDiscordEvents()
    {
      _client.Ready += ReadyAsync;
      _client.Log += LogAsync;
    }

    private async Task InitializeGlobalDataAsync()
    {
      await _globalData.InitializeAsync().ConfigureAwait(false);
    }

    private async Task ReadyAsync()
    {
      try
      {
        await _lavaNode.ConnectAsync().ConfigureAwait(false);
        await _client.SetGameAsync(GlobalData.Config.GameStatus).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        await LoggingService.LogInformationAsync(ex.Source, ex.Message).ConfigureAwait(false);
      }

    }

    private async Task LogAsync(LogMessage logMessage)
    {
      await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message).ConfigureAwait(false);
    }

    private ServiceProvider ConfigureServices()
    {
      return new ServiceCollection()
          .AddSingleton<InteractivityService>()
          .AddSingleton(new InteractivityConfig { DefaultTimeout = TimeSpan.FromSeconds(20) }) // You can optionally add a custom config
          .AddSingleton<DiscordSocketClient>()
          .AddSingleton<CommandService>()
          .AddSingleton<CommandHandler>()
          .AddSingleton<LavaNode>()
          .AddSingleton(new LavaConfig())
          .AddSingleton<LavaLinkAudio>()
          .AddSingleton<BotService>()
          .AddSingleton<GlobalData>()
          .BuildServiceProvider();
    }
  }
}
