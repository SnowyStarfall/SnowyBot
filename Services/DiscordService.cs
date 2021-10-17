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

namespace SnowyBot.Services
{
  public static class DiscordService
  {
    public static readonly DiscordSocketClient client;
    public static readonly GlobalData globalData;
    public static readonly ServiceProvider provider;
    public static readonly CommandService commands;
    public static readonly LavaNode lavaNode;
    public static readonly LavalinkModule audioModule;
    public static readonly FunModule funModule;
    public static readonly CharacterModule characterModule;
    public static readonly ConfigModule configService;
    public static readonly InteractivityService interactivity;

    public static string ownerAvatarURL;

    static DiscordService()
    {

      // Service Setup //
      provider = ConfigureServices();
      client = provider.GetRequiredService<DiscordSocketClient>();
      lavaNode = provider.GetRequiredService<LavaNode>();
      globalData = provider.GetRequiredService<GlobalData>();
      audioModule = provider.GetRequiredService<LavalinkModule>();
      funModule = provider.GetRequiredService<FunModule>();
      characterModule = provider.GetRequiredService<CharacterModule>();
      configService = provider.GetRequiredService<ConfigModule>();
      interactivity = provider.GetRequiredService<InteractivityService>();
      commands = provider.GetRequiredService<CommandService>();
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
      await Task.Delay(-1).ConfigureAwait(false);
    }
    public static void Interval()
    {
      // Create an AutoResetEvent to signal the timeout threshold in the
      // timer callback has been reached.
      var autoEvent = new AutoResetEvent(false);

      // Create a timer that invokes CheckStatus after one second, 
      // and every 1/4 second thereafter.
      Console.WriteLine("{0:h:mm:ss.fff} Creating timer.\n",
                        DateTime.Now);
      var stateTimer = new Timer((object state) =>
      {
        AutoResetEvent autoEvent = (AutoResetEvent)state;
        autoEvent.Set();
      }, autoEvent, 1000, 250);

      // When autoEvent signals, change the period to every half second.
      autoEvent.WaitOne();
      stateTimer.Change(0, 500);
      Console.WriteLine("\nChanging period to .5 seconds.\n");

      // When autoEvent signals the second time, dispose of the timer.
      autoEvent.WaitOne();
      stateTimer.Dispose();
      Console.WriteLine("\nDestroying timer.");
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
        }))
        .AddSingleton(new CommandService(new CommandServiceConfig()
        {
          LogLevel = LogSeverity.Verbose,
          DefaultRunMode = RunMode.Async,
          CaseSensitiveCommands = false
        }))
        .AddSingleton<CommandHandler>()
        .AddSingleton<EmbedHandler>()
        .AddSingleton<FunModule>()
        .AddSingleton<CharacterModule>()
        .AddSingleton<ConfigModule>()
        .AddSingleton<InteractivityService>()
        .AddSingleton(new InteractivityConfig
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
        .AddSingleton<LavalinkModule>()
        .AddSingleton<GlobalData>()
        .AddSingleton<StartupService>()
        .AddDbContext<GuildContext>()
        .AddSingleton<Guilds>()
        .AddSingleton<CharacterContext>()
        .AddSingleton<Characters>()
        .BuildServiceProvider();
    }
  }
}
