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
using System.Reflection;
using System.Collections.Generic;

namespace SnowyBot.Services
{
  public static class DiscordService
  {
    private static BotConfig config;

    public static readonly DiscordSocketClient client;
    public static readonly YoutubeClient youTube;
    public static readonly LavaNode lavaNode;
    public static readonly InteractivityService interactivity;
    public static readonly Guilds guilds;
    public static readonly CommandHandler commandHandler;

    public static readonly ServiceProvider provider;
    public static readonly ConfigModule configService;
    public static readonly CommandService commands;
    public static readonly PlaylistService playlists;
    public static readonly InteractionService interaction;

    public static readonly DevModule devModule;
    public static readonly FunModule funModule;
    public static readonly LavalinkModule lavaModule;
    public static readonly CharacterModule characterModule;
    public static readonly RoleModule roleModule;
    public static readonly PointsModule pointsModule;

    public static ConcurrentDictionary<LavaPlayer, (IGuild guild, int loop, int timer, LavaTrack trackToLoop)> tempLavaData;
    public static ConcurrentDictionary<ulong, (IUserMessage message, int timer, bool webHook, ulong author)> tempMessageData;
    public static ConcurrentDictionary<Guild, List<(ulong userID, int messages, int timer)>> tempPointsCooldownData;
    public static ConcurrentDictionary<IGuild, int> voiceChannelData;

    public static readonly Timer statusTimer;
    public static readonly Timer secondTimer;

    public static IUser Snowy;

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
      lavaModule = provider.GetRequiredService<LavalinkModule>();
      characterModule = provider.GetRequiredService<CharacterModule>();
      roleModule = provider.GetRequiredService<RoleModule>();
      pointsModule = provider.GetRequiredService<PointsModule>();

      // Lavalink Events //
      lavaNode.OnLog += Object_Log;
      lavaNode.OnTrackEnded += lavaModule.TrackEnded;

      // Discord Events //
      client.Ready += Client_Ready;
      client.Log += Object_Log;
      client.UserJoined += Client_UserJoined;
      client.UserLeft += Client_UserLeft;

      // Status Timer //
      statusTimer = new Timer(30000);
      statusTimer.Elapsed += lavaModule.StatusTumer_Elapsed;
      statusTimer.Enabled = true;

      // Second Timer //
      secondTimer = new Timer(1000);
      secondTimer.Elapsed += SecondTimer_Elapsed;
      secondTimer.Enabled = true;

      GC.KeepAlive(statusTimer);
      GC.KeepAlive(secondTimer);

      // Temp data caches //
      tempLavaData = new();
      tempMessageData = new();
      tempPointsCooldownData = new();
      voiceChannelData = new();
    }

    public static async Task InitializeAsync()
    {
      await ConfigAsync().ConfigureAwait(false);
      await client.LoginAsync(TokenType.Bot, config.DiscordToken).ConfigureAwait(false);
      await client.StartAsync().ConfigureAwait(false);
      await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider).ConfigureAwait(false);
      Snowy = await client.GetUserAsync(402246856752627713).ConfigureAwait(false);
      await Task.Delay(-1).ConfigureAwait(false);
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
      .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
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
      .AddLavaNode(x =>
      {
        x.Authorization = "crystalsmoonlightforest";
        x.EnableResume = true;
        x.ReconnectAttempts = 100;
        x.ReconnectDelay = TimeSpan.FromSeconds(5);
        x.ResumeTimeout = TimeSpan.FromSeconds(120);
        x.SelfDeaf = true;
      })
      .AddSingleton<CommandHandler>()
      .AddSingleton<EmbedHandler>()
      .AddSingleton<FunModule>()
      .AddSingleton<DevModule>()
      .AddSingleton<LavalinkModule>()
      .AddSingleton<CharacterModule>()
      .AddSingleton<RoleModule>()
      .AddSingleton<ConfigModule>()
      .AddSingleton<PointsModule>()
      .AddSingleton<StartupService>()
      .AddDbContext<GuildContext>()
      .AddSingleton<Guilds>()
      .AddSingleton<CharacterContext>()
      .AddSingleton<Characters>()
      .AddSingleton<PlaylistService>()
      .AddSingleton<YoutubeClient>()
      .BuildServiceProvider();
    }
    private static async Task Client_Ready()
    {
      await lavaNode.ConnectAsync().ConfigureAwait(false);
      await client.SetGameAsync($"music for {lavaNode.Players.Count()} servers.").ConfigureAwait(false);
    }
    private static async Task Object_Log(LogMessage logMessage)
    {
      await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message).ConfigureAwait(false);
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
    private static async void SecondTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      // Message cache handling
      if (!tempMessageData.IsEmpty)
      {
        for (int i = 0; i < tempMessageData.Count; i++)
        {
          if (tempMessageData.Count <= i)
            break;
          tempMessageData.TryUpdate(tempMessageData.ElementAt(i).Key, (tempMessageData.ElementAt(i).Value.message, tempMessageData.ElementAt(i).Value.timer - 1, tempMessageData.ElementAt(i).Value.webHook, tempMessageData.ElementAt(i).Value.author), tempMessageData.ElementAt(i).Value);
          if (tempMessageData.ElementAt(i).Value.timer <= 0)
            tempMessageData.TryRemove(tempMessageData.ElementAt(i));
        }
      }
      // XP Cooldown handling
      if (!tempPointsCooldownData.IsEmpty)
      {
        for (int i = 0; i < tempPointsCooldownData.Count; i++)
        {
          List<(ulong userID, int messages, int timer)> values = tempPointsCooldownData.ElementAt(i).Value;
          for (int k = 0; k < values.Count; k++)
          {
            (ulong userID, int messages, int timer) value = values.ElementAt(k);
            value.timer--;
            values[k] = value;
            if (value.timer <= 0)
            {
              if (value.messages > 1)
              {
                value.timer = 5;
                value.messages--;
                values[k] = value;
              }
              else
              {
                values.RemoveAt(k);
              }
            }
          }
          if (values.Count == 0)
            tempPointsCooldownData.TryRemove(tempPointsCooldownData.ElementAt(i).Key, out _);
          else
            tempPointsCooldownData.TryUpdate(tempPointsCooldownData.ElementAt(i).Key, values, tempPointsCooldownData.ElementAt(i).Value);
        }
      }
      // LavaPlayer inactivity handling
      if (!tempLavaData.IsEmpty)
      {
        for (int i = 0; i < tempLavaData.Count; i++)
        {
          if (tempLavaData.ElementAt(i).Key == null)
          {
            tempLavaData.TryRemove(tempLavaData.ElementAt(i).Key, out _);
            continue;
          }
          bool active = tempLavaData.ElementAt(i).Key.PlayerState != Victoria.Enums.PlayerState.None && tempLavaData.ElementAt(i).Key.PlayerState != Victoria.Enums.PlayerState.Stopped && tempLavaData.ElementAt(i).Key.PlayerState != Victoria.Enums.PlayerState.Paused;
          var users = (new List<IGuildUser>((await tempLavaData.ElementAt(i).Key.VoiceChannel.GetUsersAsync().ToListAsync().ConfigureAwait(false)).FirstOrDefault())).Where(x => x.VoiceChannel != null && tempLavaData.ElementAt(i).Key.VoiceChannel != null && x.VoiceChannel == tempLavaData.ElementAt(i).Key.VoiceChannel && !x.IsBot);
          if (users == null)
          {
            tempLavaData.TryRemove(tempLavaData.ElementAt(i).Key, out _);
            continue;
          }
          //if (tempUsers != null)
          //  users = users.Where(x => x.VoiceChannel != null && tempLavaData.ElementAt(i).Key.VoiceChannel != null && x.VoiceChannel == tempLavaData.ElementAt(i).Key.VoiceChannel).ToList();

          if (!users.Any() || !active)
          {
            tempLavaData.TryUpdate(tempLavaData.ElementAt(i).Key, (tempLavaData.ElementAt(i).Value.guild, tempLavaData.ElementAt(i).Value.loop, tempLavaData.ElementAt(i).Value.timer - 1, tempLavaData.ElementAt(i).Value.trackToLoop), (tempLavaData.ElementAt(i).Value.guild, tempLavaData.ElementAt(i).Value.loop, tempLavaData.ElementAt(i).Value.timer, tempLavaData.ElementAt(i).Value.trackToLoop));
            if (tempLavaData.ElementAt(i).Value.timer <= 0)
            {
              await tempLavaData.ElementAt(i).Key.TextChannel.SendMessageAsync("I have been inactive for more than five minutes. Disconnecting.").ConfigureAwait(false);
              await tempLavaData.ElementAt(i).Key.StopAsync().ConfigureAwait(false);
              await lavaNode.LeaveAsync(tempLavaData.ElementAt(i).Key.VoiceChannel).ConfigureAwait(false);
              tempLavaData.TryRemove(tempLavaData.ElementAt(i).Key, out _);
              continue;
            }
          }
          else
          {
            tempLavaData.TryUpdate(tempLavaData.ElementAt(i).Key, (tempLavaData.ElementAt(i).Value.guild, tempLavaData.ElementAt(i).Value.loop, 300, tempLavaData.ElementAt(i).Value.trackToLoop), (tempLavaData.ElementAt(i).Value.guild, tempLavaData.ElementAt(i).Value.loop, tempLavaData.ElementAt(i).Value.timer, tempLavaData.ElementAt(i).Value.trackToLoop));
          }
        }
      }
    }
  }
}
