using Discord;
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

namespace SnowyBot.Modules
{
  public sealed class LavalinkModule : ModuleBase
  {
    public EmbedBuilder builder;
    public LavalinkModule AudioService { get; set; }
    public LavalinkModule(LavaNode _lavaNode) => lavaNode = _lavaNode;
    public readonly LavaNode lavaNode;

    [Command("Join")]
    public async Task Join()
    {
      IVoiceState voiceState = Context.User as IVoiceState;
      if (lavaNode.HasPlayer(Context.Guild))
        await Context.Channel.SendMessageAsync("I'm already connected to a voice channel.").ConfigureAwait(false);

      if (voiceState.VoiceChannel is null)
        await Context.Channel.SendMessageAsync("You must be connected to a voice channel.").ConfigureAwait(false);

      await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
      await Context.Channel.SendMessageAsync($"Connected to {voiceState.VoiceChannel.Name}.").ConfigureAwait(false);
    }
    [Command("Play")]
    public async Task Play([Remainder] string query = null)
    {
      // User
      SocketGuildUser user = Context.User as SocketGuildUser;

      // Timestamp check
      int index = query.IndexOf("?t=");
      int seconds = -1;
      if (index != -1)
      {
        seconds = int.Parse(query.Remove(0, index + 5));
        query = query.Remove(index, query.Length - index);
      }

      // List of playlist videos
      List<PlaylistVideo> videos = null;

      // Is the query a playlist link?
      if (PlaylistId.TryParse(query) != null)
        videos = await DiscordService.playlists.GetPlaylistResults(query).ConfigureAwait(false);

      // Check if the server has no
      // player. If it doesn't, join.
      if (!lavaNode.HasPlayer(Context.Guild))
        await Join().ConfigureAwait(false);

      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

        List<LavaTrack> tracks = new List<LavaTrack>();

        if (videos == null)
        {
          SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ? await lavaNode.SearchAsync(SearchType.YouTube, query).ConfigureAwait(false)
                                                                                     : await lavaNode.SearchYouTubeAsync(query).ConfigureAwait(false);
          if (search.Status == SearchStatus.NoMatches)
          {
            await Context.Channel.SendMessageAsync($"No results for {query}. :x:").ConfigureAwait(false);
            return;
          }

          tracks.Add(search.Tracks.FirstOrDefault());
        }


        if (player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
        {
          if (videos != null)
          {
            foreach (PlaylistVideo video in videos)
            {
              SearchResponse response = await lavaNode.SearchYouTubeAsync(video.Title).ConfigureAwait(false);
              LavaTrack track = response.Tracks.FirstOrDefault();
              player.Queue.Enqueue(track);
            }
            await Context.Channel.SendMessageAsync($":white_check_mark: Playlist has been added to the queue.").ConfigureAwait(false);
          }
          else
          {
            player.Queue.Enqueue(tracks.FirstOrDefault());
            await Context.Channel.SendMessageAsync($":white_check_mark: {tracks.FirstOrDefault().Title} has been added to the queue.").ConfigureAwait(false);
          }
        }
        else
        {
          if (videos != null)
          {
            bool playFirst = false;
            for (int i = 0; i < videos.Count; i++)
            {
              SearchResponse response = await lavaNode.SearchYouTubeAsync(videos[i].Title).ConfigureAwait(false);
              LavaTrack track = response.Tracks.FirstOrDefault();
              player.Queue.Enqueue(track);
              if (!playFirst)
              {
                playFirst = true;
                await player.PlayAsync(player.Queue.FirstOrDefault()).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($":arrow_forward:: {track.Title}\n{track.Url}").ConfigureAwait(false);
              }
            }
            await Context.Channel.SendMessageAsync($":white_check_mark: Playlist has been added to the queue.").ConfigureAwait(false);
          }
          else
          {
            player.Queue.Enqueue(tracks.FirstOrDefault());
            await player.PlayAsync((PlayArgs args) =>
            {
              args.Track = player.Queue.FirstOrDefault();
              if (seconds != -1)
                args.StartTime = TimeSpan.FromSeconds(seconds);
            }).ConfigureAwait(false);
            await Context.Channel.SendMessageAsync($":arrow_forward:: {tracks.FirstOrDefault().Title}\n{tracks.FirstOrDefault().Url}").ConfigureAwait(false);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Leave")]
    public async Task Leave()
    {
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

        if (player == null)
        {
          await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          return;
        }

        if (player.PlayerState is PlayerState.Playing)
          await player.StopAsync().ConfigureAwait(false);

        await lavaNode.LeaveAsync(player.VoiceChannel).ConfigureAwait(false);

        await Context.Channel.SendMessageAsync(":wave:").ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("List")]
    [Alias(new string[] { "Queue" })]
    public async Task List()
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (player == null)
      {
        await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        return;
      }
      if (!(Context.User is IVoiceChannel))
        await Context.Channel.SendMessageAsync("You must be connected to a voice channel.").ConfigureAwait(false);

      int num = 1;

      EmbedBuilder builder = new EmbedBuilder();
      builder.WithTitle("Queue");
      builder.WithColor(new Color(0xcc70ff));
      builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
      foreach (LavaTrack track in player.Queue)
      {
        builder.AddField($"{EmbedHandler.NumToEmoji(num)} - {track.Title}", $"{track.Url}", false);
        if (num++ > 9) break;
      }
      if(player.Queue.Count > 10)
      builder.AddField($"**...**", $"...and {player.Queue.Count - 10} more results.", false);

      await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
      return;
    }
    [Command("QueueRemove")]
    [Alias(new string[] { "QR", "Remove", "QDelete" })]
    public async Task QRemove(int index)
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (player == null)
      {
        await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        return;
      }
      if ((player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused) || player.Queue.Count < 1)
      {
        await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        return;
      }
      if(index < 1 || index > player.Queue.Count)
      {
        await Context.Channel.SendMessageAsync("Please enter a valid index.").ConfigureAwait(false);
        return;
      }
      player.Queue.RemoveAt(index - 1);

      await Context.Message.AddReactionAsync(Emoji.Parse("👍")).ConfigureAwait(false);
    }
    [Command("Playing")]
    [Alias(new string[] { "NP", "NowPlaying", "Current" })]
    public async Task Playing()
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (player == null)
      {
        await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        return;
      }
      await Context.Channel.SendMessageAsync($"{(player.PlayerState == PlayerState.Playing ? ":arrow_forward:" : ":pause")}Now Playing: **{player.Track.Title}**\n{player.Track.Url}").ConfigureAwait(false);
      return;
    }
    [Command("Shuffle")]
    public async Task Shuffle()
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (player == null)
      {
        await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync(":twisted_rightwards_arrows:").ConfigureAwait(false);
      player.Queue.Shuffle();
    }
    [Command("Skip")]
    [Alias(new string[] { "S" })]
    public async Task Skip()
    {
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
        if (player == null)
        {
          await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          return;
        }
        if (player.Track == null)
        {
          await Context.Channel.SendMessageAsync("No songs playing.").ConfigureAwait(false);
          return;
        }
        if (player.Queue.Count < 1)
        {
          await player.StopAsync().ConfigureAwait(false);
          await Context.Channel.SendMessageAsync($":track_next::stop_button: - **{player.Track.Title}**\n({player.Track.Url})").ConfigureAwait(false);
          return;
        }
        else
        {
          await player.SkipAsync(new TimeSpan(0, 0, 0, 1, 0)).ConfigureAwait(false);
          await Context.Channel.SendMessageAsync($":track_next: - **{player.Track.Title}**\n({player.Track.Url})").ConfigureAwait(false);
          return;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Stop")]
    public async Task Stop()
    {
      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);

        if (player == null)
        {
          await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          return;
        }

        if (player.PlayerState is PlayerState.Playing)
          await player.StopAsync().ConfigureAwait(false);
        else
        {
          await Context.Channel.SendMessageAsync("Not playing anything.").ConfigureAwait(false);
          return;
        }

        await Context.Channel.SendMessageAsync(":stop_button: Stopped playback.").ConfigureAwait(false);
        return;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Volume")]
    public async Task Volume(int volume)
    {
      if (volume > 150 || volume <= 0)
      {
        await Context.Channel.SendMessageAsync("Volume must be between 1 and 150.").ConfigureAwait(false);
        return;
      }
      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);
        await Context.Channel.SendMessageAsync(volume > player.Volume ? ":loud_sound::arrow_double_up:" : volume < player.Volume ? ":sound::arrow_double_down:" : ":sound:").ConfigureAwait(false);
        await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Pause")]
    public async Task Pause()
    {
      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);
        if (!(player.PlayerState is PlayerState.Playing))
        {
          await player.PauseAsync().ConfigureAwait(false);
          await Context.Channel.SendMessageAsync("There is nothing to pause.").ConfigureAwait(false);
        }
        if (player.PlayerState is PlayerState.Paused)
        {
          await player.PauseAsync().ConfigureAwait(false);
          await Context.Channel.SendMessageAsync("Already paused.").ConfigureAwait(false);
        }
        await player.PauseAsync().ConfigureAwait(false);
        await Context.Channel.SendMessageAsync($"**:pause_button:** {player.Track.Title}.").ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Resume")]
    public async Task Resume()
    {
      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);

        if (player.PlayerState is PlayerState.Paused)
          await player.ResumeAsync().ConfigureAwait(false);

        await Context.Channel.SendMessageAsync($":arrow_forward: Resumed playback.").ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Console.Write(ex.Message);
      }
    }
    [Command("Seek")]
    [Alias(new string[] { "Find" })]
    public async Task Seek([Remainder] string time)
    {
      string[] formats = new string[] { @"s", @"ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

      bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (timespan > player.Track.Duration)
      {
        await Context.Channel.SendMessageAsync("Time extends beyond video length.").ConfigureAwait(false);
        return;
      }

      if (!valid)
      {
        await Context.Channel.SendMessageAsync("Incorrect time format.").ConfigureAwait(false);
        return;
      }

      if (!player.Track.CanSeek)
      {
        await Context.Channel.SendMessageAsync("Cannot seek in this track.").ConfigureAwait(false);
        return;
      }

      await player.SeekAsync(timespan).ConfigureAwait(false);
      await Context.Channel.SendMessageAsync($":mag_right: **{time}**").ConfigureAwait(false);
    }
    [Command("Jump")]
    public async Task Jump([Remainder] string time)
    {
      string[] formats = new string[] { @"s", @"ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

      bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

      if (!valid)
      {
        await Context.Channel.SendMessageAsync("Incorrect time format.").ConfigureAwait(false);
        return;
      }

      if (timespan > player.Track.Duration || timespan > (player.Track.Duration - player.Track.Position))
      {
        await Context.Channel.SendMessageAsync("Time extends beyond video length.").ConfigureAwait(false);
        return;
      }

      if (!player.Track.CanSeek)
      {
        await Context.Channel.SendMessageAsync("Cannot jump in this track.").ConfigureAwait(false);
        return;
      }

      await player.SeekAsync(player.Track.Position + timespan).ConfigureAwait(false);
      await Context.Channel.SendMessageAsync($":mag_right: **{player.Track.Position + timespan}**").ConfigureAwait(false);
    }
    [Command("Loop")]
    public async Task Loop()
    {
      if (!DiscordService.tempGuildData.TryGetValue(lavaNode.GetPlayer(Context.Guild), out bool l))
      {
        DiscordService.tempGuildData.GetOrAdd(lavaNode.GetPlayer(Context.Guild), true);
        await Context.Channel.SendMessageAsync($"Loop enabled! :repeat:").ConfigureAwait(false);
      }
      else
      {
        DiscordService.tempGuildData.TryGetValue(lavaNode.GetPlayer(Context.Guild), out bool loop);
        DiscordService.tempGuildData.TryUpdate(lavaNode.GetPlayer(Context.Guild), !loop, loop);
        await Context.Channel.SendMessageAsync($"Loop {(!loop ? "enabled. :repeat:" : "disabled. :arrow_right_hook:")}").ConfigureAwait(false);
      }
    }
    public async Task TrackEnded(TrackEndedEventArgs args)
    {
      if (DiscordService.tempGuildData.TryGetValue(args.Player, out bool loop) && loop && args.Reason == TrackEndReason.Finished)
      {
        LavaTrack loopTrack = args.Player.Queue.FirstOrDefault();
        await args.Player.PlayAsync(loopTrack).ConfigureAwait(false);
        return;
      }

      if (args.Reason == TrackEndReason.Stopped)
        return;

      if (args.Reason == TrackEndReason.Replaced)
        return;

      args.Player.Queue.TryDequeue(out LavaTrack value);

      if (!args.Player.Queue.Any())
        return;

      LavaTrack track = args.Player.Queue.FirstOrDefault();

      await args.Player.PlayAsync(track).ConfigureAwait(false);
      await args.Player.TextChannel.SendMessageAsync($":arrow_forward: - **{track.Title}**\n{track.Url}").ConfigureAwait(false);
    }
  }
}
