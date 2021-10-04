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
using SnowyBot.Services;
using Discord.Commands;
using Victoria.Responses.Search;
using Interactivity.Selection;
using System.Collections.Generic;
using Interactivity;
using Discord.Rest;

namespace SnowyBot.Services
{
  public sealed class LavaLinkAudio : ModuleBase<SocketCommandContext>
  {
    public EmbedBuilder builder;
    public LavaLinkAudio AudioService { get; set; }
    public LavaLinkAudio(LavaNode _lavaNode) => lavaNode = _lavaNode;
    public readonly LavaNode lavaNode;

    [Command("Join")]
    public async Task JoinAsync(SocketCommandContext context)
    {
      IVoiceState voiceState = context.User as IVoiceState;
      if (lavaNode.HasPlayer(context.Guild))
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Join", "I'm already connected to a voice channel.")).ConfigureAwait(false);

      if (voiceState.VoiceChannel is null)
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Join", "You must be connected to a voice channel.")).ConfigureAwait(false);

      await lavaNode.JoinAsync(voiceState.VoiceChannel, context.Channel as ITextChannel).ConfigureAwait(false);
      await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Join", $"Connected to {voiceState.VoiceChannel.Name}.", Color.Green)).ConfigureAwait(false);
    }
    [Command("Play")]
    public async Task PlayAsync(SocketCommandContext context, [Remainder] string query)
    {
      SocketGuildUser user = context.User as SocketGuildUser;

      if (user.VoiceChannel == null)
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Play", "You need to be in a voice channel.")).ConfigureAwait(false);

      if (!lavaNode.HasPlayer(context.Guild))
      {
        await JoinAsync(context).ConfigureAwait(false);
        //await EmbedHandler.CreateErrorEmbed("Play", "I'm not connected to a voice channel.").ConfigureAwait(false);
      }

      try
      {
        LavaPlayer player = lavaNode.GetPlayer(context.Guild);

        LavaTrack track = null;

        SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ? await lavaNode.SearchAsync(SearchType.YouTube, query).ConfigureAwait(false) :
                                                                                     await lavaNode.SearchYouTubeAsync(query).ConfigureAwait(false);

        if (search.Status == SearchStatus.NoMatches)
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music", $"No results for {query}.")).ConfigureAwait(false);
          return;
        }

        RestUserMessage message = await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateMusicListEmbed(search.Tracks)).ConfigureAwait(false);
        for (int i = 0; i < search.Tracks.Count; i++)
          await message.AddReactionAsync(new Emoji(EmbedHandler.NumToEmoji(i + 1))).ConfigureAwait(false);

        var result = await DiscordService.interactivity.NextReactionAsync(default, default, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
        if (result.IsSuccess)
        {
          for (int i = 0; i < search.Tracks.Count; i++)
          {
            if (result.Value.Emote == new Emoji(EmbedHandler.NumToEmoji(i + 1)))
            {
              track = search.Tracks.ElementAt(i);
              break;
            }
          }
        }
        else
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music", "Selection timed out.", new Color(0xcc70ff))).ConfigureAwait(false);
          return;
        }

        if ((player.Track != null && player.PlayerState is PlayerState.Playing) || player.PlayerState is PlayerState.Paused)
        {
          player.Queue.Enqueue(track);
          await LoggingService.LogInformationAsync("Music", $"Title: {track.Title}\nGuild: {context.Guild.Id}\nQuery: {query}").ConfigureAwait(false);
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to the queue.", Color.Blue)).ConfigureAwait(false);
        }
        await player.PlayAsync(track).ConfigureAwait(false);
        await LoggingService.LogInformationAsync("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}\nGuild: {context.Guild.Id}").ConfigureAwait(false);
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue)).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message)).ConfigureAwait(false);
      }
    }
    [Command("Leave")]
    public async Task LeaveAsync(SocketCommandContext context)
    {
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(context.Guild);

        if (player.PlayerState is PlayerState.Playing)
          await player.StopAsync().ConfigureAwait(false);

        await lavaNode.LeaveAsync(player.VoiceChannel).ConfigureAwait(false);

        await LoggingService.LogInformationAsync("Music", $"Bot has left. {context.Guild.Name} - {context.Guild.Id}").ConfigureAwait(false);
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music", "Goodbye!", Color.Blue)).ConfigureAwait(false);
      }
      catch (InvalidOperationException ex)
      {
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message)).ConfigureAwait(false);
      }
    }
    [Command("List")]
    public async Task ListAsync(SocketCommandContext context)
    {
      try
      {
        StringBuilder descriptionBuilder = new StringBuilder();

        LavaPlayer player = lavaNode.GetPlayer(context.Guild);
        if (player == null)
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.")).ConfigureAwait(false);
          return;
        }

        if (player.PlayerState is PlayerState.Playing)
        {
          if (player.Queue.Count < 1 && player.Track != null)
          {
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue)).ConfigureAwait(false);
            return;
          }
          else
          {
            var trackNum = 2;
            foreach (LavaTrack track in player.Queue)
            {
              descriptionBuilder.Append(trackNum)
                                .Append(": [ ")
                                .Append(track.Title)
                                .Append(" ] ( ")
                                .Append(track.Url)
                                .Append(" ) - ")
                                .Append(track.Id)
                                .Append('\n');
              trackNum++;
            }
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue)).ConfigureAwait(false);
            return;
          }
        }
        else
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, please Contact Snowy#0364.")).ConfigureAwait(false);
          return;
        }
      }
      catch (Exception ex)
      {
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, List", ex.Message)).ConfigureAwait(false);
        return;
      }

    }
    [Command("Skip")]
    public async Task SkipTrackAsync(SocketCommandContext context)
    {
      try
      {
        LavaPlayer player = lavaNode.GetPlayer(context.Guild);
        if (player == null)
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.")).ConfigureAwait(false);
          return;
        }
        if (player.Queue.Count < 1)
        {
          try
          {
            var currentTrack = player.Track;
            await player.StopAsync().ConfigureAwait(false);
            await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}. No songs left in queue.").ConfigureAwait(false);
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, SkipTrack", "Track skipped. No songs left in queue." + $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}Stop?")).ConfigureAwait(false);
            return;
          }
          catch (Exception ex)
          {
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message)).ConfigureAwait(false);
            return;
          }
        }
        else
        {
          try
          {
            var currentTrack = player.Track;
            await player.SkipAsync().ConfigureAwait(false);
            await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}").ConfigureAwait(false);
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue)).ConfigureAwait(false);
            return;
          }
          catch (Exception ex)
          {
            await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message)).ConfigureAwait(false);
            return;
          }
        }
      }
      catch (Exception ex)
      {
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message)).ConfigureAwait(false);
        return;
      }
    }
    [Command("Stop")]
    public async Task StopAsync(SocketCommandContext context)
    {
      try
      {
        var player = lavaNode.GetPlayer(context.Guild);

        if (player == null)
        {
          await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.")).ConfigureAwait(false);
          return;
        }

        /* Check if the player exists, if it does, check if it is playing.
             If it is playing, we can stop.*/
        if (player.PlayerState is PlayerState.Playing)
        {
          await player.StopAsync().ConfigureAwait(false);
        }

        await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.").ConfigureAwait(false);
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue)).ConfigureAwait(false);
        return;
      }
      catch (Exception ex)
      {
        await context.Channel.SendMessageAsync(null, false, EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message)).ConfigureAwait(false);
        return;
      }
    }
    [Command("Volume")]
    public async Task<string> SetVolumeAsync(SocketCommandContext context, int volume)
    {
      if (volume > 150 || volume <= 0)
      {
        return "Volume must be between 1 and 150.";
      }
      try
      {
        var player = lavaNode.GetPlayer(context.Guild);
        await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
        await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}").ConfigureAwait(false);
        return $"Volume has been set to {volume}.";
      }
      catch (InvalidOperationException ex)
      {
        return ex.Message;
      }
    }
    [Command("Pause")]
    public async Task<string> PauseAsync(SocketCommandContext context, IGuild guild)
    {
      try
      {
        var player = lavaNode.GetPlayer(context.Guild);
        if (!(player.PlayerState is PlayerState.Playing))
        {
          await player.PauseAsync().ConfigureAwait(false);
          return "There is nothing to pause.";
        }
        if (player.PlayerState is PlayerState.Paused)
        {
          await player.PauseAsync().ConfigureAwait(false);
          return "Already paused.";
        }
        await player.PauseAsync().ConfigureAwait(false);
        return $"**Paused:** {player.Track.Title}.";
      }
      catch (InvalidOperationException ex)
      {
        return ex.Message;
      }
    }
    [Command("Resume")]
    public async Task<string> ResumeAsync(SocketCommandContext context, IGuild guild)
    {
      try
      {
        var player = lavaNode.GetPlayer(context.Guild);

        if (player.PlayerState is PlayerState.Paused)
          await player.ResumeAsync().ConfigureAwait(false);

        return $"**Resumed:** {player.Track.Title}";
      }
      catch (InvalidOperationException ex)
      {
        return ex.Message;
      }
    }
    public async Task TrackEnded(TrackEndedEventArgs args)
    {
      if (!args.Player.Queue.TryDequeue(out var queueable))
      {
        return;
      }

      if (!(queueable is LavaTrack track))
      {
        await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.").ConfigureAwait(false);
        return;
      }

      await args.Player.PlayAsync(track).ConfigureAwait(false);
      await args.Player.TextChannel.SendMessageAsync(null, false, EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue)).ConfigureAwait(false);
    }
  }
}
