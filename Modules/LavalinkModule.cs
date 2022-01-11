﻿using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using SnowyBot.Handlers;
using Discord.Commands;
using Victoria.Responses.Search;
using Discord.Rest;
using SnowyBot.Services;
using YoutubeExplode.Playlists;
using System.Collections.Generic;
using System.Net.Http;
using System.Timers;
using SnowyBot.Database;
using static SnowyBot.SnowyBotUtils;
using Victoria.Resolvers;

namespace SnowyBot.Modules
{
  public sealed class LavalinkModule : ModuleBase
  {
    public EmbedBuilder builder;
    public LavalinkModule AudioService { get; set; }
    public readonly LavaNode lavaNode;
    public readonly Guilds guilds;
    public LavalinkModule(LavaNode _lavaNode, Guilds _guilds)
    {
      lavaNode = _lavaNode;
      guilds = _guilds;
    }

    [Command("Join")]
    [Alias(new string[] { "J" })]
    public async Task Join()
    {
      IVoiceState voiceState = Context.User as IVoiceState;
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      bool execute = await CheckUserVoice(true).ConfigureAwait(false);
      if (!execute)
        return;

      if (lavaNode.HasPlayer(Context.Guild))
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm already connected to a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      LavaPlayer player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
      if (!DiscordService.tempLavaData.ContainsKey(player))
        DiscordService.tempLavaData.TryAdd(player, (Context.Guild, 0, 300, null));
      IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} Connected to {voiceState.VoiceChannel.Name}.").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m3.DeleteAsync().ConfigureAwait(false);
      }
    }
    [Command("Play")]
    [Alias(new string[] { "P" })]
    public async Task Play([Remainder] string query = null)
    {
      if (!lavaNode.HasPlayer(Context.Guild))
      {
        LavaPlayer tempPlayer = await lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
        if (!DiscordService.tempLavaData.ContainsKey(tempPlayer))
          DiscordService.tempLavaData.TryAdd(tempPlayer, (Context.Guild, 0, 300, null));
      }

      bool execute = await CheckUserVoice().ConfigureAwait(false);
      if (!execute)
        return;

      TimeSpan startTime = TimeSpan.MinValue;
      if (query.IndexOf("?t=") != -1)
      {
        int index = query.IndexOf("?t=");
        string time = query.Remove(0, index + 3);
        query = query.Remove(index, query.Length - index);
        startTime = TimeSpan.FromSeconds(int.Parse(time));
      }

      List<PlaylistVideo> videos = PlaylistId.TryParse(query) != null ? await DiscordService.playlists.GetPlaylistResults(query).ConfigureAwait(false) : null;
      List<LavaTrack> tracks = new();
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
      SocketGuildUser user = Context.User as SocketGuildUser;
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool playing = player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused;

      if (videos == null)
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
          IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} **No results for:**\n{query}").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }
        if (search.Status == SearchStatus.LoadFailed)
        {
          IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} **Search failed.**").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m2.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }

        tracks.Add(search.Tracks.FirstOrDefault());
      }

      if (playing)
      {
        if (videos != null)
        {
          foreach (PlaylistVideo video in videos)
          {
            SearchResponse response = await lavaNode.SearchYouTubeAsync(video.Url).ConfigureAwait(false);
            LavaTrack track = response.Tracks.FirstOrDefault();
            if (track != null)
              player.Queue.Enqueue(track);
          }
          IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Playlist queued.**").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m3.DeleteAsync().ConfigureAwait(false);
          }
        }
        else
        {
          player.Queue.Enqueue(tracks.FirstOrDefault());
          IUserMessage m4 = await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Added to queue:**\n{tracks.FirstOrDefault().Title}").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m4.DeleteAsync().ConfigureAwait(false);
          }
        }
      }
      else
      {
        if (videos != null)
        {
          bool playFirst = false;
          for (int i = 0; i < videos.Count; i++)
          {
            SearchResponse response = await lavaNode.SearchYouTubeAsync(videos[i].Url).ConfigureAwait(false);
            LavaTrack track = response.Tracks.FirstOrDefault();
            if (i == 23)
            {
              int a;
            }
            if (playFirst && track != null)
              player.Queue.Enqueue(track);
            if (!playFirst)
            {
              playFirst = true;
              await player.PlayAsync(response.Tracks.FirstOrDefault()).ConfigureAwait(false);
              IUserMessage m5 = await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Now Playing:**\n{track.Title}\n{track.Url}").ConfigureAwait(false);
              if (guild.DeleteMusic)
              {
                await Task.Delay(5000).ConfigureAwait(false);
                await m5.DeleteAsync().ConfigureAwait(false);
              }
            }
          }
          IUserMessage m6 = await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Playlist queued.**").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m6.DeleteAsync().ConfigureAwait(false);
          }
        }
        else
        {
          await player.PlayAsync((PlayArgs args) =>
          {
            args.Track = tracks.FirstOrDefault();
            if (startTime != TimeSpan.MinValue && startTime <= args.Track.Duration)
              args.StartTime = startTime;
          }).ConfigureAwait(false);
          IUserMessage m7 = await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Now Playing:**\n{tracks.FirstOrDefault().Title}\n{tracks.FirstOrDefault().Url}").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m7.DeleteAsync().ConfigureAwait(false);
          }
        }
      }
    }
    public async Task Play(LavaTrack track, string query = null)
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      // Check if the server has no
      // player. If it doesn't, join.
      if (!lavaNode.HasPlayer(Context.Guild))
      {
        LavaPlayer tempPlayer = await lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
        if (!DiscordService.tempLavaData.ContainsKey(tempPlayer))
          DiscordService.tempLavaData.TryAdd(tempPlayer, (Context.Guild, 0, 300, null));
      }

      int seconds = -1;
      if (query != null)
      {
        int index = query.IndexOf("?t=");
        if (index != -1)
        {
          seconds = int.Parse(query.Remove(0, index + 5));
          query = query.Remove(index, query.Length - index);
        }
      }

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      if (player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
      {
        player.Queue.Enqueue(track);
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowySuccess} {SnowySmallButton} **Added to queue:**\n{track.Title}").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
      }
      else
      {
        await player.PlayAsync((PlayArgs args) =>
        {
          args.Track = player.Queue.FirstOrDefault();
          if (query != null && seconds != -1)
            args.StartTime = TimeSpan.FromSeconds(seconds);
        }).ConfigureAwait(false);
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Now Playing:**\n{track.Title}\n{track.Url}").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
      }
    }
    [Command("Leave")]
    [Alias(new string[] { "L", "Heckoff" })]
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
    [Command("List")]
    [Alias(new string[] { "Queue" })]
    public async Task List()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      if (player.Queue.Count == 0)
      {
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Queue is empty.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      int num = 1;

      EmbedBuilder builder = new();
      builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Queue {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
      builder.WithColor(new Color(0xcc70ff));
      builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
      builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
      builder.AddField($"{SnowyPlay} {SnowySmallButton} {player.Track.Title}", player.Track.Url, false);
      foreach (LavaTrack track in player.Queue)
      {
        builder.AddField($"{NumToDarkEmoji(num)} {SnowySmallButton} {track.Title}", track.Url, false);
        if (num++ > 9) break;
      }
      if (player.Queue.Count > 10)
        builder.AddField($"**...**", $"...and {player.Queue.Count - 10} more results.", false);

      await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
      return;
    }
    [Command("Search")]
    [Alias(new string[] { "s" })]
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
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} No results for {query}.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (search.Status == SearchStatus.LoadFailed)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Search failed.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      int num = 1;

      EmbedBuilder builder = new();
      builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Results {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
      builder.WithColor(new Color(0xcc70ff));
      builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
      builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
      builder.WithDescription("Page function incomplete for now.");
      foreach (LavaTrack track in search.Tracks)
      {
        builder.AddField($"{NumToDarkEmoji(num)} {SnowySmallButton} {track.Title}", $"{track.Url}", false);
        if (num++ > 9) break;
      }
      if (player.Queue.Count > 10)
        builder.AddField("**...**", $"...and {player.Queue.Count - 10} more results.", false);

      IUserMessage m3 = await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
      var result = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty) && (EmbedHandler.EmojiToNum(x.Content) != -1 || int.TryParse(x.Content, out _) && (int.Parse(x.Content) > 0 && int.Parse(x.Content) <= 10)), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
      if (result.IsSuccess)
      {
        int num2 = EmbedHandler.EmojiToNum(result.Value.Content);
        if (num2 == -1)
          num2 = int.Parse(result.Value.Content);
        await m3.DeleteAsync().ConfigureAwait(false);
        if (player.PlayerState is PlayerState.Paused || player.PlayerState is PlayerState.Playing)
          player.Queue.Enqueue(search.Tracks.ElementAt(num2 - 1));
        else
          await Play(search.Tracks.ElementAt(num2 - 1)).ConfigureAwait(false);
      }
      else
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Timed out or incorrect response.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m4.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
    }
    [Command("QueueRemove")]
    [Alias(new string[] { "QR", "QDelete", "Remove" })]
    public async Task QRemove(int index, int index2 = -1)
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      if (index < 1 || index > player.Queue.Count)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Please enter a valid index.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (index2 != -1)
        player.Queue.RemoveRange(index - 1, index2 - 1);
      else
        player.Queue.RemoveAt(index - 1);

      await Context.Message.AddReactionAsync(Emote.Parse(SnowyUniversalStrong)).ConfigureAwait(false);
    }
    [Command("QueueClear")]
    [Alias(new[] { "QC", "QClear", "Clear" })]
    public async Task QClear()
    {
      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      player.Queue.Clear();

      await Context.Message.AddReactionAsync(Emote.Parse(SnowyUniversalStrong)).ConfigureAwait(false);
    }
    [Command("Playing")]
    [Alias(new string[] { "NP", "NowPlaying", "Current" })]
    public async Task Playing()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      IUserMessage m3 = await Context.Channel.SendMessageAsync($"{(player.PlayerState is PlayerState.Playing ? SnowyPlay : SnowyPause)}**Now Playing:** {player.Track.Title}\n{player.Track.Url}").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m3.DeleteAsync().ConfigureAwait(false);
      }
      return;
    }
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
    [Command("Skip")]
    [Alias(new string[] { "S" })]
    public async Task Skip()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      if (player.Queue.Count == 0)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowySkipForward}{SnowyStop} {SnowySmallButton} **Finished queue.**").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
        await player.StopAsync().ConfigureAwait(false);
        return;
      }
      else
      {
        await player.SkipAsync().ConfigureAwait(false);
        await player.PlayAsync(player.Queue.First()).ConfigureAwait(false);
        IUserMessage m4 = await Context.Channel.SendMessageAsync($"{SnowySkipForward} {SnowySmallButton} **Now Playing:**\n{player.Track.Title}\n({player.Track.Url})").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m4.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
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
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyStop} **Stopped playback and cleared queue.**").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
      }
    }
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
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Volume must be between 1 and 150.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      IUserMessage m2 = await Context.Channel.SendMessageAsync(volume > player.Volume ? ":loud_sound::arrow_double_up:" : volume < player.Volume ? ":sound::arrow_double_down:" : ":sound:").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m2.DeleteAsync().ConfigureAwait(false);
      }
      await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
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
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Already paused.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
      }

      await player.PauseAsync().ConfigureAwait(false);
      IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyPause} {SnowySmallButton} **Pausing playback:**\n{player.Track.Title}.").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m2.DeleteAsync().ConfigureAwait(false);
      }
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

      IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyPlay} {SnowySmallButton} **Resuming playback.**").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m1.DeleteAsync().ConfigureAwait(false);
      }
    }
    [Command("Seek")]
    [Alias(new string[] { "Find" })]
    public async Task Seek([Remainder] string time)
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      string[] formats = new string[] { @"s", @"ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

      bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

      if (timespan > player.Track.Duration)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video length.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (!valid)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Incorrect time format.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (!player.Track.CanSeek)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Cannot seek in this track.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
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

      string[] formats = new string[] { @"s", @"ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

      bool subtract = time[0] == '-';

      time = subtract ? time.Remove(0, 1) : time;

      bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

      if (!valid)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Incorrect time format.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (timespan > player.Track.Duration || (!subtract && timespan > (player.Track.Duration - player.Track.Position)))
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video end.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (subtract && timespan > player.Track.Position)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Time extends beyond video start.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (!player.Track.CanSeek)
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} Cannot jump in this track.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m4.DeleteAsync().ConfigureAwait(false);
        }
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
        if (parsed && number > 10 || number < 1)
        {
          execute = false;
          await Context.Channel.SendMessageAsync("Value must be a number between 1-10.").ConfigureAwait(false);
        }
      }
      if (!execute)
        return;
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;
      if (!DiscordService.tempLavaData.ContainsKey(player))
      {
        DiscordService.tempLavaData.GetOrAdd(player, (Context.Guild, number, 300, player.Track));
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"{SnowyLoopEnabled} {SnowySmallButton} Loop enabled.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
      }
      else
      {
        DiscordService.tempLavaData.TryGetValue(player, out (IGuild guild, int loop, int timer, LavaTrack track) value);
        DiscordService.tempLavaData.TryUpdate(player, (value.guild, value.loop == -1 && number == -1 ? 0 : number, value.timer, value.loop == -1 && number == -1 ? null : player?.Track), (value.guild, value.loop, value.timer, value.track));

        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{(value.loop == 0 && number == -1 ? $"{SnowyLoopEnabled} {SnowySmallButton} Loop enabled." : value.loop == 0 || value.loop == -1 && number != -1 ? $"{SnowyLoopLimited} {SnowySmallButton} Loop enabled for {number} loops." : $"{SnowyLoopDisabled} {SnowySmallButton} Loop disabled.")}").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
      }
    }
    [Command("Lyrics")]
    [Alias(new[] { "Lyr" })]
    public async Task Lyrics([Remainder] string query = null)
    {
      bool execute = await CheckBotVoice().ConfigureAwait(false) && await CheckUserVoice().ConfigureAwait(false) && await CheckPlaying().ConfigureAwait(false);
      if (!execute)
        return;

      LavaPlayer player = lavaNode.HasPlayer(Context.Guild) ? lavaNode.GetPlayer(Context.Guild) : null;

      string g = (await LyricsResolver.SearchGeniusAsync(player.Track).ConfigureAwait(false)).Replace("[", "\n[");
      //string o = await LyricsResolver.SearchOvhAsync(player.Track).ConfigureAwait(false);


      //bool found = !((g == string.Empty || g == null) && (o == string.Empty || o == null));

      //if (!found)
      //{
      //  await Context.Channel.SendMessageAsync("No results.").ConfigureAwait(false);
      //  return;
      //}
      if (g != string.Empty && g != null)
      {
        await Context.Channel.SendMessageAsync(g).ConfigureAwait(false);
        return;
      }
      //if (o != string.Empty && o != null)
      //{
      //  await Context.Channel.SendMessageAsync(o).ConfigureAwait(false);
      //  return;
      //}
      //if (query == null)
      //{
      //}
      //await Context.Channel.SendMessageAsync("Who is the artist?").ConfigureAwait(false);
      //var result = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
      //if(result.IsSuccess)
      //{
      //  string g = await LyricsResolver.SearchGeniusAsync(result.Value.Content, query).ConfigureAwait(false);
      //  string o = await LyricsResolver.SearchOvhAsync(result.Value.Content, query).ConfigureAwait(false);

      //  bool found = !((g == string.Empty || g == null) && (o == string.Empty || o == null));

      //  if (!found)
      //  {
      //    await Context.Channel.SendMessageAsync("No results.").ConfigureAwait(false);
      //    return;
      //  }
      //  if (g != string.Empty && g != null)
      //  {
      //    await Context.Channel.SendMessageAsync(g).ConfigureAwait(false);
      //    return;
      //  }
      //  if (o != string.Empty && o != null)
      //  {
      //    await Context.Channel.SendMessageAsync(o).ConfigureAwait(false);
      //    return;
      //  }
      //}
      //else
      //{
      //  await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
      //}
    }
    [Command("Artwork")]
    [Alias(new[] { "art" })]
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
    public async Task TrackEnded(TrackEndedEventArgs args)
    {
      if (DiscordService.tempLavaData.TryGetValue(args.Player, out (IGuild guild, int loop, int timer, LavaTrack track) value) && value.loop != 0 && args.Reason == TrackEndReason.Finished)
      {
        if (value.loop != -1)
          DiscordService.tempLavaData.TryUpdate(args.Player, (value.guild, value.loop - 1, value.timer, value.loop - 1 == 0 ? null : value.track), value);
        await args.Player.PlayAsync(value.track).ConfigureAwait(false);
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
      IUserMessage m = await args.Player.TextChannel.SendMessageAsync($"{SnowySkipForward} {SnowySmallButton}\n{track.Title}\n{track.Url}").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m.DeleteAsync().ConfigureAwait(false);
      }

      args.Player.Queue.TryDequeue(out LavaTrack value2);
    }
    public async Task<bool> CheckBotVoice()
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      if (player == null)
      {
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
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
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} You must be connected to a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
        return false;
      }
      if (player != null && user.VoiceChannel != player.VoiceChannel && !ignoreSame)
      {
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} You must be connected to the same voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
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
        IUserMessage m = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m.DeleteAsync().ConfigureAwait(false);
        }
        return false;
      }
      if ((player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused))
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"{SnowyError} {SnowySmallButton} I'm not playing anything.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return false;
      }
      return true;
    }
    public async void StatusTumer_Elapsed(object sender, ElapsedEventArgs e)
    {
      int count = DiscordService.lavaNode.Players.Count();
      await DiscordService.client.SetGameAsync($"music for {count} server{(count == 1 ? "" : "s")}.").ConfigureAwait(false);
      await LoggingService.LogAsync("timer", LogSeverity.Info, $"Status timer elapsed. Status set to -- \"Playing music for {DiscordService.lavaNode.Players.Count()} servers.\" --").ConfigureAwait(false);
    }
  }
}
