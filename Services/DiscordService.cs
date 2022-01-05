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
using System.Timers;
using Discord.Interactions;

namespace SnowyBot.Services
{
  public static class DiscordService
  {
    public static readonly DiscordSocketClient client;

    public static readonly YoutubeClient youTube;

    public static readonly LavaNode lavaNode;

    public static readonly InteractivityService interactivity;

    public static BotConfig config;

    public static readonly Guilds guilds;

    public static readonly CommandHandler commandHandler;

    public static readonly ServiceProvider provider;
    public static readonly ConfigModule configService;
    public static readonly CommandService commands;
    public static readonly PlaylistService playlists;
    public static readonly InteractionService interaction;

    public static readonly DevModule devModule;
    public static readonly FunModule funModule;
    public static readonly LavalinkModule audioModule;
    public static readonly CharacterModule characterModule;
    public static readonly RoleModule roleModule;

    public static ConcurrentDictionary<LavaPlayer, bool> tempGuildData;
    public static ConcurrentDictionary<SocketMessage, int> tempMessageData;

    public static Timer statusTimer;
    public static Timer secondTimer;

    static DiscordService()
    {
      // Service Setup //
      provider = ConfigureServices();

      client = provider.GetRequiredService<DiscordSocketClient>();

      youTube = provider.GetRequiredService<YoutubeClient>();

      lavaNode = provider.GetRequiredService<LavaNode>();

      interactivity = provider.GetRequiredService<InteractivityService>();

      guilds = provider.GetRequiredService<Guilds>();

      commandHandler = provider.GetRequiredService<CommandHandler>();

      configService = provider.GetRequiredService<ConfigModule>();
      commands = provider.GetRequiredService<CommandService>();
      playlists = provider.GetRequiredService<PlaylistService>();

      devModule = provider.GetRequiredService<DevModule>();
      funModule = provider.GetRequiredService<FunModule>();
      audioModule = provider.GetRequiredService<LavalinkModule>();
      characterModule = provider.GetRequiredService<CharacterModule>();
      roleModule = provider.GetRequiredService<RoleModule>();

      // Lavalink Events //
      lavaNode.OnLog += Log;
      lavaNode.OnTrackEnded += audioModule.TrackEnded;

      // Discord Events //
      client.Ready += Client_Ready;
      client.Log += Log;
      client.UserJoined += Client_UserJoined;
      client.UserLeft += Client_UserLeft;

      statusTimer = new Timer(30000);
      statusTimer.Elapsed += audioModule.StatusTumer_Elapsed;
      statusTimer.Interval = 30000;
      statusTimer.Enabled = true;

      secondTimer = new Timer(1000);
      secondTimer.Elapsed += SecondTimer_Elapsed;

      GC.KeepAlive(statusTimer);

      tempGuildData = new();
      tempMessageData = new();
    }

    public static async Task InitializeAsync()
    {
      await ConfigAsync().ConfigureAwait(false);
      provider.GetRequiredService<CommandHandler>();
      provider.GetRequiredService<EmbedHandler>();
      await provider.GetRequiredService<StartupService>().StartAsync().ConfigureAwait(false);
      await Task.Delay(-1).ConfigureAwait(false);
    }
    private static async Task Client_Ready()
    {
      await lavaNode.ConnectAsync().ConfigureAwait(false);
      await client.SetGameAsync($"music for {lavaNode.Players.Count()} servers.").ConfigureAwait(false);
    }
    private static async Task Log(LogMessage logMessage)
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
          DefaultRunMode = Discord.Commands.RunMode.Async,
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
        .AddSingleton<DevModule>()
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
    private static async Task Client_UserJoined(SocketGuildUser arg)
    {
      string message = await guilds.GetWelcomeMessage(arg.Guild.Id).ConfigureAwait(false);
      if (message == null)
        return;
      string[] split = message.Split(';');
      if (split[0] == "0")
        return;
      split[1] = split[1].Replace("$MENTION$", arg.Mention);

      IMessageChannel channel = null;

      foreach (SocketGuildChannel channels in arg.Guild.Channels)
      {
        if (channels.Id == ulong.Parse(split[0]))
          channel = channels as IMessageChannel;
      }

      if (channel == null)
        return;

      await channel.SendMessageAsync(split[1]).ConfigureAwait(false);
    }
    private static async Task Client_UserLeft(SocketGuild arg1, SocketUser arg2)
    {
      string message = await guilds.GetGoodbyeMessage(arg1.Id).ConfigureAwait(false);
      if (message == null)
        return;
      string[] split = message.Split(';');
      if (split[0] == "0")
        return;
      split[1] = split[1].Replace("$MENTION$", arg2.Mention);

      IMessageChannel channel = null;

      foreach (SocketGuildChannel channels in arg1.Channels)
      {
        if (channels.Id == ulong.Parse(split[0]))
          channel = channels as IMessageChannel;
      }

      if (channel == null)
        return;

      await channel.SendMessageAsync(split[1]).ConfigureAwait(false);
    }
    private static void SecondTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      // Message cache handling
      if (tempMessageData.IsEmpty)
        return;
      for(int i = 0; i < tempMessageData.Count; i++)
      {
        tempMessageData.TryUpdate(tempMessageData.ElementAt(i).Key, tempMessageData.ElementAt(i).Value - 1, tempMessageData.ElementAt(i).Value);
        if (tempMessageData.ElementAt(i).Value <= 0)
          tempMessageData.TryRemove(tempMessageData.ElementAt(i));
      }
    }
  }
}
