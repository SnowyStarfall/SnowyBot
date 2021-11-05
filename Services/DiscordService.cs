using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SnowyBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;
using System.Linq;
using System.Threading;
using SnowyBot.Modules;
using SnowyBot.Database;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Discord.Rest;
using YoutubeExplode;
using System.Collections.Concurrent;

namespace SnowyBot.Services
{
  public static class DiscordService
  {
    public static readonly DiscordSocketClient client;

    public static readonly YoutubeClient youTube;

    public static readonly LavaNode lavaNode;

    public static readonly InteractivityService interactivity;

    public static readonly GlobalData globalData;

    public static readonly ServiceProvider provider;
    public static readonly ConfigModule configService;
    public static readonly CommandService commands;
    public static readonly PlaylistService playlists;

    public static readonly FunModule funModule;
    public static readonly LavalinkModule audioModule;
    public static readonly CharacterModule characterModule;

    public static ConcurrentDictionary<LavaPlayer, bool> tempGuildData;

    static DiscordService()
    {
      // Service Setup //
      provider = ConfigureServices();

      client = provider.GetRequiredService<DiscordSocketClient>();

      youTube = provider.GetRequiredService<YoutubeClient>();

      lavaNode = provider.GetRequiredService<LavaNode>();

      interactivity = provider.GetRequiredService<InteractivityService>();

      globalData = provider.GetRequiredService<GlobalData>();
      configService = provider.GetRequiredService<ConfigModule>();
      commands = provider.GetRequiredService<CommandService>();
      playlists = provider.GetRequiredService<PlaylistService>();

      funModule = provider.GetRequiredService<FunModule>();
      audioModule = provider.GetRequiredService<LavalinkModule>();
      characterModule = provider.GetRequiredService<CharacterModule>();

      // Lavalink Events //
      lavaNode.OnLog += LogAsync;
      lavaNode.OnTrackEnded += audioModule.TrackEnded;

      // Discord Events //
      client.Ready += ReadyAsync;
      client.Log += LogAsync;
    }

    public static async Task InitializeAsync()
    {
      await globalData.InitializeAsync().ConfigureAwait(false);
      provider.GetRequiredService<CommandHandler>();
      provider.GetRequiredService<EmbedHandler>();
      await provider.GetRequiredService<StartupService>().StartAsync().ConfigureAwait(false);

      tempGuildData = new ConcurrentDictionary<LavaPlayer, bool>();

      await Task.Delay(-1).ConfigureAwait(false);
    }
    private static async Task ReadyAsync()
    {
      await lavaNode.ConnectAsync().ConfigureAwait(false);
      await client.SetGameAsync($"music for {lavaNode.Players.Count()} servers.").ConfigureAwait(false);
    }
    private static async Task LogAsync(LogMessage logMessage)
    {
      await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message).ConfigureAwait(false);
    }
    private static ServiceProvider ConfigureServices()
    {
      return new ServiceCollection()
        .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
          LogLevel = LogSeverity.Verbose,
          AlwaysDownloadUsers = true,
          GatewayIntents = GatewayIntents.All,
        }))
        .AddSingleton(new CommandService(new CommandServiceConfig()
        {
          LogLevel = LogSeverity.Verbose,
          DefaultRunMode = RunMode.Async,
          CaseSensitiveCommands = false
        }))
        .AddSingleton<InteractivityService>()
        .AddSingleton(new InteractivityConfig()
        {
          DefaultTimeout = TimeSpan.FromSeconds(20)
        })
        .AddSingleton<LavaNode>()
        .AddSingleton(new LavaConfig()
        {
          Authorization = "crystalsmoonlightforest",
          EnableResume = true,
          ReconnectAttempts = 10,
          ReconnectDelay = TimeSpan.FromSeconds(5),
          ResumeTimeout = TimeSpan.FromSeconds(120),
          SelfDeaf = true
        })
        .AddSingleton(new YouTubeService(new BaseClientService.Initializer()
        {
          ApiKey = "AIzaSyCoFA16Gt7lHoX4rJHVOBH6Jh77xW6Je78",
          ApplicationName = "SnowyBot",
        }))
        .AddSingleton<CommandHandler>()
        .AddSingleton<EmbedHandler>()
        .AddSingleton<FunModule>()
        .AddSingleton<CharacterModule>()
        .AddSingleton<ConfigModule>()
        .AddSingleton<LavalinkModule>()
        .AddSingleton<GlobalData>()
        .AddSingleton<StartupService>()
        .AddDbContext<GuildContext>()
        .AddSingleton<Guilds>()
        .AddSingleton<CharacterContext>()
        .AddSingleton<Characters>()
        .AddSingleton<PlaylistService>()
        .AddSingleton<YoutubeClient>()
        .BuildServiceProvider();
    }
  }
}
