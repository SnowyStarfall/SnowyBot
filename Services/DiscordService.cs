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
  public static class DiscordService
  {
    public static readonly DiscordSocketClient client;
    public static readonly CommandHandler commandHandler;
    public static readonly ServiceProvider service;
    public static readonly LavaNode lavaNode;
    public static readonly LavaLinkAudio audioService;
    public static readonly GlobalData globalData;
    public static readonly InteractivityService interactivity;

    static DiscordService()
    {
      service = ConfigureServices();
      client = service.GetRequiredService<DiscordSocketClient>();
      commandHandler = service.GetRequiredService<CommandHandler>();
      lavaNode = service.GetRequiredService<LavaNode>();
      globalData = service.GetRequiredService<GlobalData>();
      audioService = service.GetRequiredService<LavaLinkAudio>();
      interactivity = service.GetRequiredService<InteractivityService>();

      SubscribeLavaLinkEvents();
      SubscribeDiscordEvents();
    }

    public static async Task InitializeAsync()
    {
      await InitializeGlobalDataAsync().ConfigureAwait(false);

      await client.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken).ConfigureAwait(false);
      await client.StartAsync().ConfigureAwait(false);

      await commandHandler.InitializeAsync().ConfigureAwait(false);

      await Task.Delay(-1).ConfigureAwait(false);
    }

    private static void SubscribeLavaLinkEvents()
    {
      lavaNode.OnLog += LogAsync;
      lavaNode.OnTrackEnded += audioService.TrackEnded;
    }

    private static void SubscribeDiscordEvents()
    {
      client.Ready += ReadyAsync;
      client.Log += LogAsync;
    }

    private static async Task InitializeGlobalDataAsync()
    {
      await globalData.InitializeAsync().ConfigureAwait(false);
    }

    private static async Task ReadyAsync()
    {
      await lavaNode.ConnectAsync().ConfigureAwait(false);
      await client.SetGameAsync(GlobalData.Config.GameStatus).ConfigureAwait(false);
    }

    private static async Task LogAsync(LogMessage logMessage)
    {
      await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message).ConfigureAwait(false);
    }

    private static ServiceProvider ConfigureServices()
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
