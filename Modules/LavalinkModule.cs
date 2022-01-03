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
using System.Timers;
using SnowyBot.Database;

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
      if (lavaNode.HasPlayer(Context.Guild))
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm already connected to a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
      }

      if (voiceState.VoiceChannel is null)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("You must be connected to a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
      }

      await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel).ConfigureAwait(false);
      IUserMessage m3 = await Context.Channel.SendMessageAsync($"Connected to {voiceState.VoiceChannel.Name}.").ConfigureAwait(false);
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
      // User
      SocketGuildUser user = Context.User as SocketGuildUser;
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

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
          //if(!Uri.IsWellFormedUriString(query, UriKind.Absolute))
          //  return;

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
            IUserMessage m1 = await Context.Channel.SendMessageAsync($"No results for {query}. :x:").ConfigureAwait(false);
            if (guild.DeleteMusic)
            {
              await Task.Delay(5000).ConfigureAwait(false);
              await m1.DeleteAsync().ConfigureAwait(false);
            }
            return;
          }
          if (search.Status == SearchStatus.LoadFailed)
          {
            IUserMessage m2 = await Context.Channel.SendMessageAsync("Search failed. :x:").ConfigureAwait(false);
            if (guild.DeleteMusic)
            {
              await Task.Delay(5000).ConfigureAwait(false);
              await m2.DeleteAsync().ConfigureAwait(false);
            }
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
            IUserMessage m3 = await Context.Channel.SendMessageAsync($":white_check_mark: Playlist has been added to the queue.").ConfigureAwait(false);
            if (guild.DeleteMusic)
            {
              await Task.Delay(5000).ConfigureAwait(false);
              await m3.DeleteAsync().ConfigureAwait(false);
            }
          }
          else
          {
            player.Queue.Enqueue(tracks.FirstOrDefault());
            IUserMessage m4 = await Context.Channel.SendMessageAsync($":white_check_mark: {tracks.FirstOrDefault().Title} has been added to the queue.").ConfigureAwait(false);
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
              SearchResponse response = await lavaNode.SearchYouTubeAsync(videos[i].Title).ConfigureAwait(false);
              LavaTrack track = response.Tracks.FirstOrDefault();
              player.Queue.Enqueue(track);
              if (!playFirst)
              {
                playFirst = true;
                await player.PlayAsync(player.Queue.FirstOrDefault()).ConfigureAwait(false);
                IUserMessage m5 = await Context.Channel.SendMessageAsync($":arrow_forward:: {track.Title}\n{track.Url}").ConfigureAwait(false);
                if (guild.DeleteMusic)
                {
                  await Task.Delay(5000).ConfigureAwait(false);
                  await m5.DeleteAsync().ConfigureAwait(false);
                }
              }
            }
            IUserMessage m6 = await Context.Channel.SendMessageAsync($":white_check_mark: Playlist has been added to the queue.").ConfigureAwait(false);
            if (guild.DeleteMusic)
            {
              await Task.Delay(5000).ConfigureAwait(false);
              await m6.DeleteAsync().ConfigureAwait(false);
            }
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
            IUserMessage m7 = await Context.Channel.SendMessageAsync($":arrow_forward:: {tracks.FirstOrDefault().Title}\n{tracks.FirstOrDefault().Url}").ConfigureAwait(false);
            if (guild.DeleteMusic)
            {
              await Task.Delay(5000).ConfigureAwait(false);
              await m7.DeleteAsync().ConfigureAwait(false);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    public async Task Play(LavaTrack track, string query = null)
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      // Check if the server has no
      // player. If it doesn't, join.
      if (!lavaNode.HasPlayer(Context.Guild))
        await Join().ConfigureAwait(false);
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

      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

        if (player.Track != null || player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
        {
          player.Queue.Enqueue(track);
          IUserMessage m1 = await Context.Channel.SendMessageAsync($":white_check_mark: {track.Title} has been added to the queue.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
        }
        else
        {
          player.Queue.Enqueue(track);
          await player.PlayAsync((PlayArgs args) =>
          {
            args.Track = player.Queue.FirstOrDefault();
            if (query != null && seconds != -1)
              args.StartTime = TimeSpan.FromSeconds(seconds);
          }).ConfigureAwait(false);
          IUserMessage m2 = await Context.Channel.SendMessageAsync($":arrow_forward:: {track.Title}\n{track.Url}").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m2.DeleteAsync().ConfigureAwait(false);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Leave")]
    [Alias(new string[] { "L", "Heckoff" })]
    public async Task Leave()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

        if (player == null)
        {
          IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }

        if (player.PlayerState is PlayerState.Playing)
          await player.StopAsync().ConfigureAwait(false);

        await lavaNode.LeaveAsync(player.VoiceChannel).ConfigureAwait(false);

        await Context.Message.AddReactionAsync(Emoji.Parse("👋")).ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (player == null)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (Context.User is not IVoiceChannel)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync("You must be connected to a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
      }

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

      // Check if the server has no
      // player. If it doesn't, join.
      if (!lavaNode.HasPlayer(Context.Guild))
        await Join().ConfigureAwait(false);

      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);

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
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"No results for {query}. :x:").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (search.Status == SearchStatus.LoadFailed)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("Search failed. :x:").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      int num = 1;

      EmbedBuilder builder = new EmbedBuilder();
      builder.WithTitle("Queue");
      builder.WithColor(new Color(0xcc70ff));
      builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713ul).ConfigureAwait(false)).GetAvatarUrl(ImageFormat.Gif) ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
      builder.WithDescription("Page function incomplete for now.");
      foreach (LavaTrack track in search.Tracks)
      {
        builder.AddField($"{EmbedHandler.NumToEmoji(num)} - {track.Title}", $"{track.Url}", false);
        if (num++ > 9) break;
      }
      if (player.Queue.Count > 10)
        builder.AddField("**...**", $"...and {player.Queue.Count - 10} more results.", false);

      IUserMessage m3 = await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);

      var result = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty) && (EmbedHandler.EmojiToNum(x.Content) != -1 || (int.Parse(x.Content) > 0 && int.Parse(x.Content) <= 10)), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (result.IsSuccess)
      {
        int num2 = EmbedHandler.EmojiToNum(result.Value.Content);
        if (num2 == -1)
          num2 = int.Parse(result.Value.Content);
        await Play(search.Tracks.ElementAt(num2 - 1)).ConfigureAwait(false);
        await m3.DeleteAsync().ConfigureAwait(false);
      }
      else
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync("Timed out or incorrect response.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m4.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
    }
    [Command("QueueRemove")]
    [Alias(new string[] { "QR", "Remove", "QDelete" })]
    public async Task QRemove(int index)
    {
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (player == null)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if ((player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused) || player.Queue.Count < 1)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (index < 1 || index > player.Queue.Count)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync("Please enter a valid index.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (player == null)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      IUserMessage m3 = await Context.Channel.SendMessageAsync($"{(player.PlayerState == PlayerState.Playing ? ":arrow_forward:" : ":pause")}Now Playing: **{player.Track.Title}**\n{player.Track.Url}").ConfigureAwait(false);
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
      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (player == null)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not in a voice channel.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("I'm not playing anything.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      await Context.Message.AddReactionAsync(Emoji.Parse("🔀")).ConfigureAwait(false);
      player.Queue.Shuffle();
    }
    [Command("Skip")]
    [Alias(new string[] { "S" })]
    public async Task Skip()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
        if (player == null)
        {
          IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }
        if (player.Track == null)
        {
          IUserMessage m2 = await Context.Channel.SendMessageAsync("No songs playing.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m2.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }
        if (player.Queue.Count <= 1)
        {
          IUserMessage m3 = await Context.Channel.SendMessageAsync($":track_next::stop_button: - **Finished queue.**").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m3.DeleteAsync().ConfigureAwait(false);
          }
          await player.StopAsync().ConfigureAwait(false);
          player.Queue.Clear();
          return;
        }
        else
        {
          await player.SkipAsync().ConfigureAwait(false);
          await player.PlayAsync(player.Queue.First()).ConfigureAwait(false);
          IUserMessage m4 = await Context.Channel.SendMessageAsync($":track_next: - **{player.Track.Title}**\n({player.Track.Url})").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m4.DeleteAsync().ConfigureAwait(false);
          }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);

        if (player == null)
        {
          IUserMessage m1 = await Context.Channel.SendMessageAsync("I'm not connected to a voice channel.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }

        if (player.PlayerState is PlayerState.Playing)
          await player.StopAsync().ConfigureAwait(false);
        else
        {
          IUserMessage m2 = await Context.Channel.SendMessageAsync("Not playing anything.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m2.DeleteAsync().ConfigureAwait(false);
          }
          return;
        }

        IUserMessage m3 = await Context.Channel.SendMessageAsync(":stop_button: Stopped playback.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      if (volume > 150 || volume <= 0)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("Volume must be between 1 and 150.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }
      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);
        IUserMessage m2 = await Context.Channel.SendMessageAsync(volume > player.Volume ? ":loud_sound::arrow_double_up:" : volume < player.Volume ? ":sound::arrow_double_down:" : ":sound:").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);
        if (!(player.PlayerState is PlayerState.Playing))
        {
          await player.PauseAsync().ConfigureAwait(false);
          IUserMessage m1 = await Context.Channel.SendMessageAsync("There is nothing to pause.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
          }
        }
        if (player.PlayerState is PlayerState.Paused)
        {
          await player.PauseAsync().ConfigureAwait(false);
          IUserMessage m2 = await Context.Channel.SendMessageAsync("Already paused.").ConfigureAwait(false);
          if (guild.DeleteMusic)
          {
            await Task.Delay(5000).ConfigureAwait(false);
            await m2.DeleteAsync().ConfigureAwait(false);
          }
        }
        await player.PauseAsync().ConfigureAwait(false);
        IUserMessage m3 = await Context.Channel.SendMessageAsync($"**:pause_button:** {player.Track.Title}.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    [Command("Resume")]
    public async Task Resume()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      try
      {
        var player = lavaNode.GetPlayer(Context.Guild);

        if (player.PlayerState is PlayerState.Paused)
          await player.ResumeAsync().ConfigureAwait(false);

        IUserMessage m1 = await Context.Channel.SendMessageAsync($":arrow_forward: Resumed playback.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (timespan > player.Track.Duration)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("Time extends beyond video length.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      if (!valid)
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("Incorrect time format.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      if (!player.Track.CanSeek)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync("Cannot seek in this track.").ConfigureAwait(false);
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
      string[] formats = new string[] { @"s", @"ss", @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };

      bool subtract = time[0] == '-';

      time = subtract ? time.Remove(0, 1) : time;

      bool valid = TimeSpan.TryParseExact(time, formats, null, out TimeSpan timespan);

      LavaPlayer player = lavaNode.GetPlayer(Context.Guild);
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      if (!valid)
      {
        IUserMessage m1 = await Context.Channel.SendMessageAsync("Incorrect time format.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      if (timespan > player.Track.Duration || (!subtract && timespan > (player.Track.Duration - player.Track.Position)))
      {
        IUserMessage m2 = await Context.Channel.SendMessageAsync("Time extends beyond video end.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      if (subtract && timespan > player.Track.Position)
      {
        IUserMessage m3 = await Context.Channel.SendMessageAsync("Time extends beyond video start.").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m3.DeleteAsync().ConfigureAwait(false);
        }
        return;
      }

      if (!player.Track.CanSeek)
      {
        IUserMessage m4 = await Context.Channel.SendMessageAsync("Cannot jump in this track.").ConfigureAwait(false);
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
    public async Task Loop()
    {
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      if (!DiscordService.tempGuildData.TryGetValue(lavaNode.GetPlayer(Context.Guild), out bool l))
      {
        DiscordService.tempGuildData.GetOrAdd(lavaNode.GetPlayer(Context.Guild), true);
        IUserMessage m1 = await Context.Channel.SendMessageAsync($"Loop enabled! :repeat:").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m1.DeleteAsync().ConfigureAwait(false);
        }
      }
      else
      {
        DiscordService.tempGuildData.TryGetValue(lavaNode.GetPlayer(Context.Guild), out bool loop);
        DiscordService.tempGuildData.TryUpdate(lavaNode.GetPlayer(Context.Guild), !loop, loop);
        IUserMessage m2 = await Context.Channel.SendMessageAsync($"Loop {(!loop ? "enabled. :repeat:" : "disabled. :arrow_right_hook:")}").ConfigureAwait(false);
        if (guild.DeleteMusic)
        {
          await Task.Delay(5000).ConfigureAwait(false);
          await m2.DeleteAsync().ConfigureAwait(false);
        }
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
      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);

      await args.Player.PlayAsync(track).ConfigureAwait(false);
      IUserMessage m1 = await args.Player.TextChannel.SendMessageAsync($":arrow_forward: - **{track.Title}**\n{track.Url}").ConfigureAwait(false);
      if (guild.DeleteMusic)
      {
        await Task.Delay(5000).ConfigureAwait(false);
        await m1.DeleteAsync().ConfigureAwait(false);
      }
    }
    public async void StatusTumer_Elapsed(object sender, ElapsedEventArgs e)
    {
      await DiscordService.client.SetGameAsync($"music for {DiscordService.lavaNode.Players.Count()} servers.").ConfigureAwait(false);
      await LoggingService.LogAsync("timer", LogSeverity.Info, $"Status timer elapsed. Status set to -- \"Playing music for {DiscordService.lavaNode.Players.Count()} servers.\" --").ConfigureAwait(false);
    }
  }
}
