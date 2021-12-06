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
using SnowyBot.Modules;
using SnowyBot.Database;
using YoutubeExplode;
using System.Collections.Concurrent;
using SnowyBot.Structs;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SnowyBot.Services
{
  public static class DiscordService
  {
    public static readonly DiscordSocketClient client;

    public static readonly YoutubeClient youTube;

    public static readonly LavaNode lavaNode;

    public static readonly InteractivityService interactivity;

    public static BotConfig config;

    public static readonly ServiceProvider provider;
    public static readonly ConfigModule configService;
    public static readonly CommandService commands;
    public static readonly PlaylistService playlists;

    public static readonly FunModule funModule;
    public static readonly LavalinkModule audioModule;
    public static readonly CharacterModule characterModule;
    public static readonly RoleModule roleModule;

    public static ConcurrentDictionary<LavaPlayer, bool> tempGuildData;

    static DiscordService()
    {
      // Service Setup //
      provider = ConfigureServices();

      client = provider.GetRequiredService<DiscordSocketClient>();

      youTube = provider.GetRequiredService<YoutubeClient>();

      lavaNode = provider.GetRequiredService<LavaNode>();

      interactivity = provider.GetRequiredService<InteractivityService>();

      configService = provider.GetRequiredService<ConfigModule>();
      commands = provider.GetRequiredService<CommandService>();
      playlists = provider.GetRequiredService<PlaylistService>();

      funModule = provider.GetRequiredService<FunModule>();
      audioModule = provider.GetRequiredService<LavalinkModule>();
      characterModule = provider.GetRequiredService<CharacterModule>();
      roleModule = provider.GetRequiredService<RoleModule>();

      // Lavalink Events //
      lavaNode.OnLog += LogAsync;
      lavaNode.OnTrackEnded += audioModule.TrackEnded;

      // Discord Events //
      client.Ready += ReadyAsync;
      client.Log += LogAsync;
    }

    public static async Task InitializeAsync()
    {
      await ConfigAsync().ConfigureAwait(false);
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
    private static async Task ConfigAsync()
    {
      await Task.Run(() =>
      {
        string json = File.ReadAllText("config.json", new UTF8Encoding(false));
        config = JsonConvert.DeserializeObject<BotConfig>(json);
      }).ConfigureAwait(false);
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
        .AddSingleton<CommandHandler>()
        .AddSingleton<EmbedHandler>()
        .AddSingleton<FunModule>()
        .AddSingleton<LavalinkModule>()
        .AddSingleton<CharacterModule>()
        .AddSingleton<RoleModule>()
        .AddSingleton<ConfigModule>()
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
