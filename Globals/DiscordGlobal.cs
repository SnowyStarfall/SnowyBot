using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SnowyBot.Database;
using SnowyBot.Modules;
using SnowyBot.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Victoria;
using Victoria.Enums;
using YoutubeExplode;
using static SnowyBot.Utilities;

namespace SnowyBot.Services
{
	public static class DiscordGlobal
	{
		// Configuration &
		public static BotConfig config;
		public static CancellationToken botAlive;

		// Clients, Nodes, & Main Handlers
		public static readonly ServiceProvider provider;
		public static readonly DiscordShardedClient client;
		public static readonly YoutubeClient youTube;
		public static readonly LavaNode lavaNode;
		public static readonly InteractivityService interactivity;

		// Commands
		public static readonly CommandService commandService;
		public static readonly CommandGlobal commandHandler;
		public static readonly InteractionService interaction;

		public static readonly Guilds guilds;
		public static readonly Characters characters;

		// Command Modules
		public static readonly DevModule devModule;
		public static readonly ConfigModule configModule;
		public static readonly FunModule funModule;
		public static readonly LavalinkModule lavaModule;
		public static readonly CharacterModule characterModule;
		public static readonly PointsModule pointsModule;

		// Temporary Data
		public static List<LavaData> lavaData;
		public static List<MessageData> messageData;
		public static List<PointCooldownData> pointCooldownData;
		public static List<Paginator> paginators;
		public static ConcurrentDictionary<ulong, (string commands, int stacks, int tries, int cooldown)> activeCommands;

		// Timers
		public static readonly System.Timers.Timer secondTimer;
		public static readonly System.Timers.Timer minuteTimer;

		// Owner
		public static IUser Snowy;

		static DiscordGlobal()
		{
			// Service Setup
			provider = ConfigureServices();
			client = provider.GetRequiredService<DiscordShardedClient>();
			youTube = provider.GetRequiredService<YoutubeClient>();
			lavaNode = provider.GetRequiredService<LavaNode>();
			interactivity = provider.GetRequiredService<InteractivityService>();
			guilds = provider.GetRequiredService<Guilds>();
			commandHandler = provider.GetRequiredService<CommandGlobal>();
			configModule = provider.GetRequiredService<ConfigModule>();
			commandService = provider.GetRequiredService<CommandService>();
			devModule = provider.GetRequiredService<DevModule>();
			funModule = provider.GetRequiredService<FunModule>();
			lavaModule = provider.GetRequiredService<LavalinkModule>();
			characterModule = provider.GetRequiredService<CharacterModule>();
			pointsModule = provider.GetRequiredService<PointsModule>();

			// Lavalink Events
			lavaNode.OnLog += Log;
			lavaNode.OnTrackEnded += lavaModule.TrackEnded;

			// Discord Events
			client.ShardReady += ShardReady;
			client.Log += Log;
			client.UserJoined += UserJoined;
			client.UserLeft += UserLeft;

			// Second Timer
			secondTimer = new(1000);
			secondTimer.Elapsed += SecondTimer;
			secondTimer.Enabled = true;

			// Minute Timer
			minuteTimer = new(60000);
			minuteTimer.Elapsed += MinuteTimer;
			minuteTimer.Enabled = true;

			GC.KeepAlive(secondTimer);
			GC.KeepAlive(minuteTimer);

			// Equalizers
			ConfigureEQ();

			// Temp data caches
			lavaData = new();
			messageData = new();
			pointCooldownData = new();
			paginators = new();
		}

		// Startup
		public static async Task InitializeAsync()
		{
			await ConfigAsync().ConfigureAwait(false);
			await client.LoginAsync(TokenType.Bot, config.DiscordToken).ConfigureAwait(false);
			await client.StartAsync().ConfigureAwait(false);
			await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider).ConfigureAwait(false);
			await Task.Delay(-1, botAlive).ConfigureAwait(false);
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
			.AddSingleton(new DiscordShardedClient(new DiscordSocketConfig()
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
			.AddSingleton<CommandGlobal>()
			.AddSingleton<FunModule>()
			.AddSingleton<DevModule>()
			.AddSingleton<LavalinkModule>()
			.AddSingleton<CharacterModule>()
			.AddSingleton<ConfigModule>()
			.AddSingleton<PointsModule>()
			.AddDbContext<GuildContext>()
			.AddSingleton<Guilds>()
			.AddSingleton<CharacterContext>()
			.AddSingleton<Characters>()
			.AddSingleton<YoutubeClient>()
			.BuildServiceProvider();
		}

		// Events
		private static async Task ShardReady(DiscordSocketClient arg)
		{
			if (Snowy == null)
				Snowy = await arg.GetUserAsync(402246856752627713).ConfigureAwait(false);
			await lavaNode.ConnectAsync().ConfigureAwait(false);

			string path = Assembly.GetExecutingAssembly().Location + "Database/LavaNodeData.lava";
			path = path.Replace("bin\\Debug\\net6.0\\SnowyBot.dll", "").Replace("\\", "/");
			if (File.Exists(path))
			{
				await LoggingGlobal.LogAsync("resume", LogSeverity.Info, "Resumption data foumd. Beginning resumption proces...").ConfigureAwait(false);
				LavaTable table = LavaTable.ReadFromBinaryFile<LavaTable>(path);
				foreach (var item in table.table)
				{
					if (item.Value == null)
						continue;

					IGuild guild = client.GetGuild(item.Key);
					if (guild == null)
						continue;

					IVoiceChannel voice = await guild.GetVoiceChannelAsync(item.Value.voice).ConfigureAwait(false);
					ITextChannel text = await guild.GetTextChannelAsync(item.Value.text).ConfigureAwait(false);
					if (voice == null || text == null)
						continue;

					LavaPlayer player = await lavaNode.JoinAsync(voice, text).ConfigureAwait(false);
					await LoggingGlobal.LogAsync("resume", LogSeverity.Info, $"Data found for Guild {guild.Id}, Voice {voice.Id}, with Text {text.Id}").ConfigureAwait(false);

					await player.UpdateVolumeAsync((ushort)item.Value.volume).ConfigureAwait(false);
					await LoggingGlobal.LogAsync("resume", LogSeverity.Info, $"Player volume set to {item.Value.volume}").ConfigureAwait(false);

					if (item.Value.playerState != PlayerState.None && item.Value.playerState != PlayerState.Stopped)
					{
						TimeSpan difference = DateTime.Now - item.Value.offlineTime;

						bool playNext = difference <= TimeSpan.FromMinutes(2) && item.Value.Position + difference > item.Value.Duration;
						if (!playNext)
						{
							item.Value.Position += difference > TimeSpan.FromMinutes(2) ? TimeSpan.FromMinutes(2) : difference;
							await LoggingGlobal.LogAsync("resume", LogSeverity.Info, $"Playing track for: {player.TextChannel.GuildId}").ConfigureAwait(false);
						}

						LavaTrack track = new(item.Value.Hash, item.Value.Id, item.Value.Title, item.Value.Author, item.Value.Url, item.Value.Position, (long)item.Value.Duration.TotalMilliseconds, item.Value.CanSeek, item.Value.IsStream, item.Value.Source);
						await player.PlayAsync((PlayArgs args) =>
						{
							args.Track = track;
							args.StartTime = track.Position;
						}).ConfigureAwait(false);

						if (item.Value.playerState == PlayerState.Paused)
							await player.PauseAsync().ConfigureAwait(false);
						await LoggingGlobal.LogAsync("resume", LogSeverity.Info, $"PlayerState: {player.PlayerState}").ConfigureAwait(false);

						foreach (string s in item.Value.queue)
						{
							string[] t = s.Split(';');
							LavaTrack queueTrack = new(t[0], t[1], t[2], t[3], t[4], TimeSpan.Parse(t[5]), (long)TimeSpan.Parse(t[6]).TotalMilliseconds, bool.Parse(t[7]), bool.Parse(t[8]), t[9]);

							if (playNext)
								await player.PlayAsync(queueTrack).ConfigureAwait(false);
							else
								player.Queue.Enqueue(queueTrack);

							await LoggingGlobal.LogAsync("resume", LogSeverity.Info, $"{(playNext ? "Playing" : "Queueing")} track for: {player.TextChannel.GuildId}").ConfigureAwait(false);

							playNext = false;
						}
					}
				}
				await LoggingGlobal.LogAsync("resume", LogSeverity.Info, "Resumption complete. Deleting data file...").ConfigureAwait(false);
				File.Delete(path);
			}
			MinuteTimer(null, null);
		}
		private static async Task Log(LogMessage logMessage)
		{
			await LoggingGlobal.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message).ConfigureAwait(false);
		}
		private static async Task UserJoined(SocketGuildUser arg)
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
		private static async Task UserLeft(SocketGuild arg1, SocketUser arg2)
		{
			string message = await guilds.GetGoodbyeMessage(arg1.Id).ConfigureAwait(false);
			if (message == null)
				return;
			string[] split = message.Split(';');
			if (split[0] == "0")
				return;
			split[1] = split[1].Replace("$MENTION$", arg2.Mention + " -- " + arg2.Username);

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

		// Timers
		private static async void SecondTimer(object sender, ElapsedEventArgs e)
		{
			// Message cache handling
			if (messageData.Count > 0)
			{
				for (int i = 0; i < messageData.Count; i++)
				{
					if (messageData.Count <= i)
						break;
					messageData[i].timer--;
					if (messageData[i].timer <= 0)
					{
						messageData.RemoveAt(i);
						i--;
					}
				}
			}

			// XP Cooldown handling
			if (pointCooldownData.Count > 0)
			{
				for (int i = 0; i < pointCooldownData.Count; i++)
				{
					PointCooldownData data = pointCooldownData[i];
					data.timer--;
					if (data.timer <= 0 && data.messages > 1)
					{
						data.timer = 5;
						data.messages--;
					}
					else
					{
						pointCooldownData.RemoveAt(i);
						i--;
					}
				}
			}

			// LavaPlayer inactivity handling
			if (lavaData.Count > 0)
			{
				for (int i = 0; i < lavaData.Count; i++)
				{
					LavaData data = lavaData[i];

					if (data.player == null)
					{
						lavaData.RemoveAt(i);
						i--;
						continue;
					}
					bool active = data.player.PlayerState != PlayerState.None && data.player.PlayerState != PlayerState.Stopped && data.player.PlayerState != PlayerState.Paused;
					IEnumerable<IGuildUser> users = null;
					if (data.player?.VoiceChannel != null)
						users = (new List<IGuildUser>((await data.player.VoiceChannel.GetUsersAsync().ToListAsync().ConfigureAwait(false)).FirstOrDefault())).Where(x => x.VoiceChannel != null && data.player.VoiceChannel != null && x.VoiceChannel == data.player.VoiceChannel && !x.IsBot);
					if (users == null)
					{
						lavaData.RemoveAt(i);
						i--;
						continue;
					}
					if (!users.Any() || !active)
					{
						data.timer--;
						if (data.timer <= 0)
						{
							await data.player.TextChannel.SendMessageAsync("I have been inactive for more than five minutes. Disconnecting.").ConfigureAwait(false);
							await data.player.StopAsync().ConfigureAwait(false);
							await lavaNode.LeaveAsync(data.player.VoiceChannel).ConfigureAwait(false);
							lavaData.RemoveAt(i);
							i--;
							continue;
						}
					}
					else
					{
						data.timer = 300;
					}
				}
			}

			// Paginator handling
			if (paginators.Count > 0)
			{
				for (int i = 0; i < paginators.Count; i++)
				{
					Paginator paginator = paginators[i];
					paginator.timer--;
					if (paginator.timer <= 0)
						paginators.RemoveAt(i);
				}
			}

			// TODO: Active command spam prevention handling
			if (activeCommands?.IsEmpty == false)
			{
				//for (int i = 0; i < activeCommands.Count; i++)
				//{
				//	if (activeCommands.ElementAt(i).Value.cooldown > 0)
				//		activeCommands.TryUpdate(activeCommands.ElementAt(i).Key, (activeCommands.ElementAt(i).Value.commands, activeCommands.ElementAt(i).Value.cooldown - 1), (activeCommands.ElementAt(i).Value.commands, activeCommands.ElementAt(i).Value.cooldown));
				//}
			}
		}
		private static async void MinuteTimer(object sender, ElapsedEventArgs e)
		{
			int lavaCount = lavaNode.Players.Count();
			int shardCount = client.Shards.Count;
			int guildCount = client.Guilds.Count;
			string status = "Invite link is now public! Check my profile. 💜 " +
							$"Playing music for {lavaCount} server{(lavaCount == 1 ? "" : "s")}. " +
							$"Hosting {shardCount} shard{(shardCount == 1 ? "" : "s")} for {guildCount} guild{(guildCount == 1 ? "" : "s")}. ";

			await client.SetGameAsync(status).ConfigureAwait(false);
			await LoggingGlobal.LogAsync("timer", LogSeverity.Info, $"Status timer elapsed. Status set to: {status}.").ConfigureAwait(false);
		}
	}
}
