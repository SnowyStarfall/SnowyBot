using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using SnowyBot.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Filters;
using Victoria.Resolvers;
using Victoria.Responses.Search;
using YoutubeExplode.Playlists;
using static SnowyBot.Utilities;

namespace SnowyBot.Modules
{
	public class LavalinkModule : ModuleBase
	{
		public readonly LavaNode lavaNode;
		public readonly Guilds guilds;

		public EmbedBuilder builder;
		public EqualizerBand[] normalEQ;
		public EqualizerBand[] bassBoostEQ;

		public LavalinkModule(LavaNode lavaNode, Guilds guilds)
		{
			this.lavaNode = lavaNode;
			this.guilds = guilds;
		}

		// Playback Commands
		[Command("Join")]
		public async Task Join()
		{
			IVoiceState voiceState = Context.User as IVoiceState;
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			bool execute = await CheckUserVoice(true).ConfigureAwait(false);
			if (!execute)
				return;

			if (lavaNode.HasPlayer(Context.Guild))
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm already connected to a voice channel.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			LavaPlayer player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
			if (DiscordGlobal.lavaData.Find(x => x.player == player) == null)
			{
				LavaData data = new()
				{
					player = player,
					loop = 0,
					timer = 300,
					track = null
				};
				DiscordGlobal.lavaData.Add(data);
			}

			await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} Connected to {voiceState.VoiceChannel.Name}.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
		}
		[Command("Play")]
		public async Task Play([Remainder] string query = null)
		{
			LavaPlayer player = null;
			if (!lavaNode.HasPlayer(Context.Guild))
				player = await lavaNode.JoinAsync((Context.User as IVoiceState)?.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
			else
				player = lavaNode.GetPlayer(Context.Guild);

			if (DiscordGlobal.lavaData.Find(x => x.player == player) == null)
			{
				LavaData data = new()
				{
					player = player,
					loop = 0,
					timer = 300,
					track = null
				};
				DiscordGlobal.lavaData.Add(data);
			}

			bool execute = await CheckUserVoice().ConfigureAwait(false) && query != null && query != string.Empty;
			if (!execute)
				return;

			ProcessTimestamp(ref query, out int startTime);

			List<LavaTrack> tracks = new();
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			List<PlaylistVideo> playlist = PlaylistId.TryParse(query) != null ? await DiscordGlobal.youTube.Playlists.GetVideosAsync(PlaylistId.Parse(query)).ToListAsync().ConfigureAwait(false) : null;

			bool playing = player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused;

			if (playlist == null)
			{
				SearchResponse YouTube = await lavaNode.SearchYouTubeAsync(query.Replace("https://youtu.be/", "https://www.youtube.com/watch?v=")).ConfigureAwait(false);
				SearchResponse YouTubeMusic = await lavaNode.SearchAsync(SearchType.YouTubeMusic, query).ConfigureAwait(false);
				SearchResponse SoundCloud = await lavaNode.SearchSoundCloudAsync(query.Replace("https://soundcloud.com/", "").Replace("/", " ").Replace("-", " ")).ConfigureAwait(false);
				SearchResponse Direct = await lavaNode.SearchAsync(SearchType.Direct, query).ConfigureAwait(false);

				SearchResponse search = YouTube.Status != SearchStatus.LoadFailed && YouTube.Status != SearchStatus.NoMatches ? YouTube :
										YouTubeMusic.Status != SearchStatus.LoadFailed && YouTubeMusic.Status != SearchStatus.NoMatches ? YouTubeMusic :
										SoundCloud.Status != SearchStatus.LoadFailed && SoundCloud.Status != SearchStatus.NoMatches ? SoundCloud :
										Direct;

				if (search.Status == SearchStatus.NoMatches || search.Tracks.Count == 0)
				{
					await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} **No results for:**\n{query}. It may be unlisted or private.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
					return;
				}
				if (search.Status == SearchStatus.LoadFailed)
				{
					await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} **Search failed.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
					return;
				}

				tracks.Add(search.Tracks.FirstOrDefault());
			}

			if (playlist != null)
			{
				ProcessPlaylist(player, playlist, !playing);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Playlist queued.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
			else if (playing)
			{
				player.Queue.Enqueue(tracks.FirstOrDefault());
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Added to queue:**\n{tracks.FirstOrDefault().Title}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
			else
			{
				await player.PlayAsync((PlayArgs args) =>
				{
					args.Track = tracks.FirstOrDefault();
					if (startTime != -1 && TimeSpan.FromSeconds(startTime) <= args.Track.Duration)
						args.StartTime = TimeSpan.FromSeconds(startTime);
				}).ConfigureAwait(false);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Now Playing:**\n{tracks.FirstOrDefault().Title}\n{tracks.FirstOrDefault().Url}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
		}
		[Command("Pause")]
		public async Task Pause()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState is PlayerState.Paused)
			{
				await player.PauseAsync().ConfigureAwait(false);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Already paused.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}

			await player.PauseAsync().ConfigureAwait(false);
			await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyPause} {SnowySmallButton} **Pausing playback:**\n{player.Track.Title}.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
		}
		[Command("Resume")]
		public async Task Resume()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState is PlayerState.Paused)
				await player.ResumeAsync().ConfigureAwait(false);

			await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Resuming playback.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
		}
		[Command("Stop")]
		public async Task Stop()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState is PlayerState.Playing)
			{
				await player.StopAsync().ConfigureAwait(false);
				player.Queue.Clear();
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyStop} **Stopped playback and cleared queue.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
		}
		[Command("Leave")]
		public async Task Leave()
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState is PlayerState.Playing)
				await player.StopAsync().ConfigureAwait(false);

			await lavaNode.LeaveAsync(player.VoiceChannel).ConfigureAwait(false);

			await Context.Message.AddReactionAsync(Emoji.Parse("👋")).ConfigureAwait(false);
		}

		// Navigation Commands
		[Command("Skip")]
		public async Task Skip()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			LavaData data = DiscordGlobal.lavaData.Find(x => x.player == player);
			if (data?.loop != 0)
			{
				data.loop = 0;
				data.track = null;
			}

			if (player.Queue.Count == 0)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySkipForward}{SnowyStop} {SnowySmallButton} **Finished queue.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				await player.StopAsync().ConfigureAwait(false);
				return;
			}
			else
			{
				await player.PlayAsync(player.Queue.First()).ConfigureAwait(false);
				await player.SkipAsync().ConfigureAwait(false);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySkipForward} {SnowySmallButton} **Now Playing:**\n{player.Track.Title}\n({player.Track.Url})").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
		}
		[Command("Seek")]
		public async Task Seek([Remainder] string time)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			string[] formats = new string[] { "s", "ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

			bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

			if (timespan > player.Track.Duration)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video length.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (!valid)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Incorrect time format.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (!player.Track.CanSeek)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Cannot seek in this track.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			await player.SeekAsync(timespan).ConfigureAwait(false);
			IUserMessage m4 = await Context.Channel.SendMessageAsync($":mag_right: **{time}**").ConfigureAwait(false);
			if (guild.DeleteMusic)
			{
				await Task.Delay(5000).ConfigureAwait(false);
				await m4.DeleteAsync().ConfigureAwait(false);
			}
		}
		[Command("Jump")]
		public async Task Jump([Remainder] string time)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			string[] formats = new string[] { "s", "ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

			bool subtract = time[0] == '-';

			time = subtract ? time.Remove(0, 1) : time;

			bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

			if (!valid)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Incorrect time format.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (timespan > player.Track.Duration || (!subtract && timespan > (player.Track.Duration - player.Track.Position)))
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video end.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (subtract && timespan > player.Track.Position)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video start.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (!player.Track.CanSeek)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Cannot jump in this track.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			string result = (subtract ? player.Track.Position - timespan : player.Track.Position + timespan).ToString();

			result = result.Remove(result.IndexOf("."), result.Length - result.IndexOf("."));

			await player.SeekAsync(subtract ? player.Track.Position - timespan : player.Track.Position + timespan).ConfigureAwait(false);
			IUserMessage m5 = await Context.Channel.SendMessageAsync($":mag_right: **{result}**").ConfigureAwait(false);
			if (guild.DeleteMusic)
			{
				await Task.Delay(5000).ConfigureAwait(false);
				await m5.DeleteAsync().ConfigureAwait(false);
			}
		}
		[Command("Loop")]
		public async Task Loop([Remainder] string input = null)
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			int number = -1;
			if (input != null)
			{
				bool parsed = int.TryParse(input, out number);
				if (!parsed)
				{
					execute = false;
					await Context.Channel.SendMessageAsync("Value must be a number.").ConfigureAwait(false);
				}
				if ((parsed && number > 10) || number < 1)
				{
					execute = false;
					await Context.Channel.SendMessageAsync("Value must be a number between 1-10.").ConfigureAwait(false);
				}
			}
			if (!execute)
				return;
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;
			if (DiscordGlobal.lavaData.Find(x => x.player == player) == null)
			{
				LavaData data = new()
				{
					player = player,
					loop = number,
					timer = 300,
					track = player.Track,
				};
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyLoopEnabled} {SnowySmallButton} Loop enabled.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
			else
			{
				LavaData data = DiscordGlobal.lavaData.Find(x => x.player == player);
				data.loop = data.loop == -1 && number == -1 ? 0 : number;
				data.track = data.loop == -1 && number == -1 ? null : player?.Track;

				await TimedMessage(await Context.Channel.SendMessageAsync($"{(data.loop == 0 && number == -1 ? $"{SnowyLoopEnabled} {SnowySmallButton} Loop enabled." : data.loop == 0 || (data.loop == -1 && number != -1) ? $"{SnowyLoopLimited} {SnowySmallButton} Loop enabled for {number} loops." : $"{SnowyLoopDisabled} {SnowySmallButton} Loop disabled.")}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
		}

		// Info Commands
		[Command("List")]
		public async Task List()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.Queue.Count == 0 && player.PlayerState is not PlayerState.Playing)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Queue is empty.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaData data = DiscordGlobal.lavaData.Find(x => x.player == player);
			bool cached = data != null;

			double quotient = player.Track.Position / player.Track.Duration;
			string position = player.Track.Position.ToString();
			int index = position.IndexOf('.');
			int count = (int)(quotient * 10);
			string time = $"**{(index == -1 ? position : position.Remove(index))}** {SnowyLineLeftEnd}";
			for (int i = 0; i < 10; i++)
			{
				if (i == count)
					time += SnowyButtonConnected;
				else
					time += SnowyLine;
			}
			time += $"{SnowyLineRightEnd} **{player.Track.Duration}**";

			int embedsNeeded = player.Queue.Count > 10 ? (player.Queue.Count / 10) + 1 : 1;
			int embedNum = 0;

			List<Embed> embeds = new();

			for (int i = 0; i < embedsNeeded; i++)
			{
				EmbedBuilder builder = new();
				builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Queue {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
				builder.WithColor(new Color(0xcc70ff));
				builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
				builder.WithFooter("Bot made by SnowyStarfall - Snowy#8364", DiscordGlobal.Snowy.GetAvatarUrl(ImageFormat.Png));
				if (i == 0)
					builder.AddField($"{SnowyPlay} {SnowySmallButton} {player.Track.Title}", $"{player.Track.Url}\n{time}{(cached && data.loop == -1 ? "\n**Looped infinitely.**" : cached && data.loop > 0 ? $"\n**Looped x {data.loop}.**" : "")}", false);
				for (int k = 0; k < 10; k++)
				{
					int index1 = (embedNum * 10) + k;
					if (index1 == player.Queue.Count)
						break;
					LavaTrack track = player.Queue.ElementAt(index1);
					string emojis = StringToNumbers(index1 + 1) ?? NumToDarkEmoji(index1 + 1);
					builder.AddField($"{emojis} {SnowySmallButton} {track.Title}", track.Url, false);
				}
				TimeSpan timeSpan = TimeSpan.Zero;
				foreach (LavaTrack track2 in player.Queue)
					timeSpan += track2.Duration;
				timeSpan += player.Track.Duration - player.Track.Position;
				string t = timeSpan.ToString();
				t = t.Remove(t.IndexOf('.'), t.Length - t.IndexOf('.'));
				builder.AddField(SnowyBlank, $"**Queue Duration: {t} left!**");
				embeds.Add(builder.Build());
				embedNum++;
			}

			string[] c = new[] { $"PreviousPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"PreviousPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}" };

			if (embedsNeeded > 1)
			{
				ComponentBuilder cBuilder = new();
				cBuilder.WithButton(null, c[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
				cBuilder.WithButton(null, c[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				IUserMessage message = await Context.Channel.SendMessageAsync(null, false, embeds[0], null, null, null, cBuilder.Build()).ConfigureAwait(false);
				Paginator page = new(embeds, message, c, 600);
				DiscordGlobal.paginators.Add(page);
				return;
			}
			await Context.Channel.SendMessageAsync(null, false, embeds[0]).ConfigureAwait(false);
			return;
		}
		[Command("Search")]
		public async Task Search([Remainder] string query)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			if (!lavaNode.HasPlayer(Context.Guild))
				await Join().ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			SearchResponse YouTube = await lavaNode.SearchYouTubeAsync(query.Replace("https://youtu.be/", "https://www.youtube.com/watch?v=")).ConfigureAwait(false);
			SearchResponse YouTubeMusic = await lavaNode.SearchAsync(SearchType.YouTubeMusic, query).ConfigureAwait(false);
			SearchResponse SoundCloud = await lavaNode.SearchSoundCloudAsync(query.Replace("https://soundcloud.com/", "").Replace("/", " ").Replace("-", " ")).ConfigureAwait(false);
			SearchResponse Direct = await lavaNode.SearchAsync(SearchType.Direct, query).ConfigureAwait(false);

			SearchResponse search = YouTube.Status != SearchStatus.LoadFailed && YouTube.Status != SearchStatus.NoMatches ? YouTube :
															YouTubeMusic.Status != SearchStatus.LoadFailed && YouTubeMusic.Status != SearchStatus.NoMatches ? YouTubeMusic :
															SoundCloud.Status != SearchStatus.LoadFailed && SoundCloud.Status != SearchStatus.NoMatches ? SoundCloud :
															Direct;

			if (search.Status == SearchStatus.NoMatches || search.Tracks.Count == 0)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} No results for {query}.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (search.Status == SearchStatus.LoadFailed)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Search failed.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			int embedsNeeded = search.Tracks.Count > 10 ? (search.Tracks.Count / 10) + 1 : 1;
			int embedNum = 0;

			List<Embed> embeds = new();

			for (int i = 0; i < embedsNeeded; i++)
			{
				EmbedBuilder builder = new();
				builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Results {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
				builder.WithColor(new Color(0xcc70ff));
				builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
				builder.WithFooter("Bot made by SnowyStarfall - Snowy#8364", DiscordGlobal.Snowy.GetAvatarUrl(ImageFormat.Png));
				for (int k = 0; k < 10; k++)
				{
					int index1 = (embedNum * 10) + k;
					if (index1 == search.Tracks.Count)
						break;
					LavaTrack track = search.Tracks.ElementAt(index1);
					string emojis = StringToNumbers(index1 + 1) ?? NumToDarkEmoji(index1 + 1);
					builder.AddField($"{emojis} {SnowySmallButton} {track.Title}", track.Url, false);
				}
				embeds.Add(builder.Build());
				embedNum++;
			}
			string[] c = new[] { $"PreviousPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"PreviousPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}" };

			if (embedsNeeded > 1)
			{
				ComponentBuilder cBuilder = new();
				cBuilder.WithButton(null, c[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
				cBuilder.WithButton(null, c[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				IUserMessage message = await Context.Channel.SendMessageAsync(null, false, embeds[0], null, null, null, cBuilder.Build()).ConfigureAwait(false);
				Paginator page = new(embeds, message, c, 600);
				DiscordGlobal.paginators.Add(page);
			}
			else
			{
				await Context.Channel.SendMessageAsync(null, false, embeds[0]).ConfigureAwait(false);
			}
			var result = await DiscordGlobal.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty) && (int.TryParse(x.Content, out _) && (int.Parse(x.Content) > 0 && int.Parse(x.Content) <= search.Tracks.Count - 1)), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (result.IsSuccess)
			{
				int num2 = int.Parse(result.Value.Content);
				if (player.PlayerState is PlayerState.Paused || player.PlayerState is PlayerState.Playing)
					player.Queue.Enqueue(search.Tracks.ElementAt(num2 - 1));
				else
					await Play(search.Tracks.ElementAt(num2 - 1)).ConfigureAwait(false);
			}
			else
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Timed out or incorrect response.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
		}
		[Command("NP")]
		public async Task NowPlaying()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			LavaData data = DiscordGlobal.lavaData.Find(x => x.player == player);
			bool cached = data != null;

			double quotient = player.Track.Position / player.Track.Duration;
			string position = player.Track.Position.ToString();
			int index = position.IndexOf('.');
			int count = (int)(quotient * 10);
			string time = $"**{(index == -1 ? position : position.Remove(index))}** {SnowyLineLeftEnd}";
			for (int i = 0; i < 10; i++)
			{
				if (i == count)
					time += SnowyButtonConnected;
				else
					time += SnowyLine;
			}
			time += $"{SnowyLineRightEnd} **{player.Track.Duration}**";

			await TimedMessage(await Context.Channel.SendMessageAsync($"{(player.PlayerState is PlayerState.Playing ? SnowyPlay : SnowyPause)}** Now Playing:** {player.Track.Title}\n{player.Track.Url}\n{time}{(cached && data.loop == -1 ? "\n**Looped infinitely.**" : cached && data.loop > 0 ? $"\n**Looped x {data.loop}.**" : "")}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			return;
		}
		[Command("Lyrics")]
		public async Task Lyrics()
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			string g = (await LyricsResolver.SearchGeniusAsync(player.Track).ConfigureAwait(false)).Replace("[", "\n[");

			if (g != string.Empty && g != null)
			{
				await Context.Channel.SendMessageAsync(g).ConfigureAwait(false);
				return;
			}
		}
		[Command("Artwork")]
		public async Task Artwork()
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			string s = await ArtworkResolver.FetchAsync(player.Track).ConfigureAwait(false);

			if (s != string.Empty && s != null)
				await Context.Channel.SendMessageAsync(s).ConfigureAwait(false);
			else
				await Context.Channel.SendMessageAsync("No results.");
		}

		// Queue Commands
		[Command("Shuffle")]
		public async Task Shuffle()
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			await Context.Message.AddReactionAsync(Emote.Parse(SnowyShuffle)).ConfigureAwait(false);
			player.Queue.Shuffle();
		}
		[Command("Remove")]
		public async Task Remove(int index, int index2 = -1)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (index < 1 || index > player.Queue.Count)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Please enter a valid index.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}
			if (index2 != -1)
				player.Queue.RemoveRange(index - 1, index2 - 1);
			else
				player.Queue.RemoveAt(index - 1);

			await Context.Message.AddReactionAsync(Emote.Parse(SnowyUniversalStrong)).ConfigureAwait(false);
		}
		[Command("Clear")]
		public async Task Clear()
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			player.Queue.Clear();

			await Context.Message.AddReactionAsync(Emote.Parse(SnowyUniversalStrong)).ConfigureAwait(false);
		}

		// Config Commands
		[Command("Volume")]
		public async Task Volume(int volume)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (volume > 150 || volume <= 0)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Volume must be between 1 and 150.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return;
			}

			await TimedMessage(await Context.Channel.SendMessageAsync(volume > player.Volume ? ":loud_sound::arrow_double_up:" : volume < player.Volume ? ":sound::arrow_double_down:" : ":sound:").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
		}
		[Command("EQ")]
		public async Task Equalize([Remainder] string eq)
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
			{
				await Context.Channel.SendMessageAsync("I must be playing a track to apply an EQ.").ConfigureAwait(false);
				return;
			}
			switch (eq.ToLower())
			{
				case string s when s == "bassboost" || s == "bb":
					await player.ApplyFiltersAsync(new List<IFilter>(), 1, bassBoostEQ);
					await Context.Channel.SendMessageAsync("Protect your ears!").ConfigureAwait(false);
					return;
				case string s when s == "normal" || s == "default":
					await player.ApplyFiltersAsync(new List<IFilter>(), 1, normalEQ);
					await Context.Channel.SendMessageAsync("Returning to normal.").ConfigureAwait(false);
					return;
			}
			await Context.Channel.SendMessageAsync("I don't recognize that equalizer. Use `!help eq` for info on equalizers.").ConfigureAwait(false);
		}
		[Command("Filter")]
		public async Task Filter([Remainder] string filter)
		{
			bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
			if (!execute)
				return;

			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
			{
				await Context.Channel.SendMessageAsync("I must be playing a track to apply a filter.").ConfigureAwait(false);
				return;
			}
			switch (filter.ToLower())
			{
				case string s when s == "vibrato" || s == "vibrate":
					VibratoFilter vibrato = new();
					vibrato.Frequency = 10;
					vibrato.Depth = 1;
					EqualizerBand[] bands = player.Equalizer.ToArray();
					await player.ApplyFiltersAsync(new List<IFilter>(), 1, normalEQ);
					await player.ApplyFilterAsync(vibrato, 1, bands);
					await Context.Channel.SendMessageAsync("Protect your ears!").ConfigureAwait(false);
					return;
			}
			await Context.Channel.SendMessageAsync("I don't recognize that filter. Use `!help filter` for info on filters.").ConfigureAwait(false);
		}

		// Utilities
		public void ProcessPlaylist(LavaPlayer player, List<PlaylistVideo> playlist, bool playFirst)
		{
			static int SortList((LavaTrack track, int index) x, (LavaTrack track, int index) y) => x.index < y.index ? -1 : x.index > y.index ? 1 : 0;

			List<(LavaTrack track, int index)> sortingList = new();

			ParallelLoopResult result = Parallel.For(0, playlist.Count, (int num) =>
			{
				SearchResponse response = lavaNode.SearchYouTubeAsync(playlist[num].Title + " " + playlist[num].Author).Result;
				if (response.Status == SearchStatus.NoMatches || response.Status == SearchStatus.LoadFailed)
					return;
				LavaTrack track = response.Tracks.FirstOrDefault();
				if (track != null)
				{
					LoggingGlobal.LogAsync("PRLL", LogSeverity.Info, num.ToString());
					if (num == 0 && playFirst)
						Play(track);
					else
						sortingList.Add((track, num));
				}
			});

			if (result.IsCompleted)
			{
				sortingList.Sort(SortList);

				for (int i = 0; i < sortingList.Count; i++)
				{
					player.Queue.Enqueue(sortingList[i].track);
					LoggingGlobal.LogAsync("LIST", LogSeverity.Info, sortingList[i].index.ToString() + " " + sortingList[i].track.Title + "\n");
				}
			}
		}
		public void ProcessTimestamp(ref string query, out int seconds)
		{
			if (query != null)
			{
				int index = query.IndexOf("?t=");
				if (index != -1)
				{
					seconds = int.Parse(query.Remove(0, index + 5));
					query = query.Remove(index, query.Length - index);
					return;
				}
			}
			seconds = -1;
		}
		public async Task Play(LavaTrack track, string query = null)
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

			LavaPlayer player = null;
			if (!lavaNode.HasPlayer(Context.Guild))
				player = await lavaNode.JoinAsync((Context.User as IVoiceState)?.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
			else
				player = lavaNode.GetPlayer(Context.Guild);

			if (DiscordGlobal.lavaData.Find(x => x.player == player) == null)
			{
				LavaData data = new()
				{
					player = player,
					loop = 0,
					timer = 300,
					track = null
				};
				DiscordGlobal.lavaData.Add(data);
			}

			ProcessTimestamp(ref query, out int startTime);

			if (player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
			{
				player.Queue.Enqueue(track);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Added to queue:**\n{track.Title}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
			else
			{
				await player.PlayAsync((PlayArgs args) =>
				{
					args.Track = track;
					if (startTime != -1 && TimeSpan.FromSeconds(startTime) <= args.Track.Duration)
						args.StartTime = TimeSpan.FromSeconds(startTime);
				}).ConfigureAwait(false);
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Now Playing:**\n{track.Title}\n{track.Url}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
			}
		}

		// Checks
		public async Task<bool> CheckBotVoice()
		{
			LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			if (player == null)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not in a voice channel.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return false;
			}
			return true;
		}
		public async Task<bool> CheckUserVoice(bool ignoreSame = false)
		{
			SocketGuildUser user = Context.User as SocketGuildUser;
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (user.VoiceChannel is null)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} You must be connected to a voice channel.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return false;
			}
			if (player != null && user.VoiceChannel != player.VoiceChannel && !ignoreSame)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} You must be connected to the same voice channel.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return false;
			}
			return true;
		}
		public async Task<bool> CheckPlaying()
		{
			Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
			LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

			if (player == null)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not in a voice channel.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return false;
			}
			if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
			{
				await TimedMessage(await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not playing anything.").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
				return false;
			}
			return true;
		}

		// Events
		public async Task TrackEnded(TrackEndedEventArgs args)
		{
			LavaData data = DiscordGlobal.lavaData.Find(x => x.player == args.Player);
			if (data?.loop != 0 && args.Reason == TrackEndReason.Finished)
			{
				if (data.loop != -1)
				{
					data.loop--;
					data.track = data.loop == 0 ? null : data.track;
				}
				await args.Player.PlayAsync(data.track).ConfigureAwait(false);
				return;
			}

			if (args.Reason == TrackEndReason.Stopped)
				return;

			if (args.Reason == TrackEndReason.Replaced)
				return;

			if (!args.Player.Queue.Any())
				return;

			LavaTrack track = args.Player.Queue.FirstOrDefault();
			Guild guild = await guilds.GetGuild((args.Player.VoiceChannel as SocketVoiceChannel).Guild.Id).ConfigureAwait(false);
			await args.Player.PlayAsync(track).ConfigureAwait(false);
			await TimedMessage(await args.Player.TextChannel.SendMessageAsync($"{SnowySkipForward} {SnowySmallButton} **Now Playing:**\n{track.Title}\n{track.Url}").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);

			args.Player.Queue.TryDequeue(out _);

			if (args.Player.Queue.Count == 0 && args.Player.Track == null)
				await TimedMessage(await args.Player.TextChannel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Queue finished.**").ConfigureAwait(false), 5000, guild.DeleteMusic).ConfigureAwait(false);
		}
	}
}