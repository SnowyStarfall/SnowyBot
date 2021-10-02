using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Victoria.Responses.Rest;
using SnowyBot.Handlers;
using SnowyBot.Services;

namespace SnowyBot.Services
{
  public sealed class LavaLinkAudio
  {
    private readonly LavaNode _lavaNode;

    public LavaLinkAudio(LavaNode lavaNode) => _lavaNode = lavaNode;

    public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
    {
      if (_lavaNode.HasPlayer(guild))
        return await EmbedHandler.CreateErrorEmbed("Join", "I'm already connected to a voice channel.").ConfigureAwait(false);

      if (voiceState.VoiceChannel is null)
        return await EmbedHandler.CreateErrorEmbed("Join", "You must be connected to a voice channel.").ConfigureAwait(false);

      await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel).ConfigureAwait(false);
      return await EmbedHandler.CreateBasicEmbed("Join", $"Connected to {voiceState.VoiceChannel.Name}.", Color.Green).ConfigureAwait(false);
    }
    public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query)
    {
      if (user.VoiceChannel == null)
        return await EmbedHandler.CreateErrorEmbed("Join - Play", "You need to be in a voice channel.").ConfigureAwait(false);

      if (!_lavaNode.HasPlayer(guild))
        return await EmbedHandler.CreateErrorEmbed("Play", "I'm not connected to a voice channel.").ConfigureAwait(false);

      try
      {
        var player = _lavaNode.GetPlayer(guild);

        LavaTrack track;

        var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
            await _lavaNode.SearchAsync(query).ConfigureAwait(false)
            : await _lavaNode.SearchYouTubeAsync(query).ConfigureAwait(false);

        if (search.LoadStatus == LoadStatus.NoMatches)
          return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.").ConfigureAwait(false);

        //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
        track = search.Tracks.FirstOrDefault();

        //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
        if ((player.Track != null && player.PlayerState is PlayerState.Playing) || player.PlayerState is PlayerState.Paused)
        {
          player.Queue.Enqueue(track);
          await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the queue.").ConfigureAwait(false);
          return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to the queue.", Color.Blue).ConfigureAwait(false);
        }

        //Player was not playing anything, so lets play the requested track.
        await player.PlayAsync(track).ConfigureAwait(false);
        await LoggingService.LogInformationAsync("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}").ConfigureAwait(false);
        return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue).ConfigureAwait(false);
      }

      //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
      catch (Exception ex)
      {
        return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message).ConfigureAwait(false);
      }

    }

    /*This is ran when a user uses the command Leave.
        Task Returns an Embed which is used in the command call. */
    public async Task<Embed> LeaveAsync(IGuild guild)
    {
      try
      {
        //Get The Player Via GuildID.
        var player = _lavaNode.GetPlayer(guild);

        //if The Player is playing, Stop it.
        if (player.PlayerState is PlayerState.Playing)
        {
          await player.StopAsync().ConfigureAwait(false);
        }

        //Leave the voice channel.
        await _lavaNode.LeaveAsync(player.VoiceChannel).ConfigureAwait(false);

        await LoggingService.LogInformationAsync("Music", "Bot has left.").ConfigureAwait(false);
        return await EmbedHandler.CreateBasicEmbed("Music", "Goodbye!", Color.Blue).ConfigureAwait(false);
      }
      //Tell the user about the error so they can report it back to us.
      catch (InvalidOperationException ex)
      {
        return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message).ConfigureAwait(false);
      }
    }

    /*This is ran when a user uses the command List 
        Task Returns an Embed which is used in the command call. */
    public async Task<Embed> ListAsync(IGuild guild)
    {
      try
      {
        /* Create a string builder we can use to format how we want our list to be displayed. */
        var descriptionBuilder = new StringBuilder();

        /* Get The Player and make sure it isn't null. */
        var player = _lavaNode.GetPlayer(guild);
        if (player == null)
          return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.").ConfigureAwait(false);

        if (player.PlayerState is PlayerState.Playing)
        {
          /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
              In this situation we simply return an embed that displays the current track instead. */
          if (player.Queue.Count < 1 && player.Track != null)
          {
            return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue).ConfigureAwait(false);
          }
          else
          {
            /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
             *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
            var trackNum = 2;
            foreach (LavaTrack track in player.Queue)
            {
              descriptionBuilder.Append(trackNum)
                                .Append(": [")
                                .Append(track.Title)
                                .Append("](")
                                .Append(track.Url)
                                .Append(") - ")
                                .Append(track.Id)
                                .Append('\n');
              trackNum++;
            }
            return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue).ConfigureAwait(false);
          }
        }
        else
        {
          return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Snowy#0364.").ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message).ConfigureAwait(false);
      }

    }

    /*This is ran when a user uses the command Skip 
        Task Returns an Embed which is used in the command call. */
    public async Task<Embed> SkipTrackAsync(IGuild guild)
    {
      try
      {
        var player = _lavaNode.GetPlayer(guild);
        /* Check if the player exists */
        if (player == null)
          return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.").ConfigureAwait(false);
        /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
             User is expected to use the Stop command if they're only wanting to skip the current song. */
        if (player.Queue.Count < 1)
        {
          return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", "Unable To skip a track as there is only One or No songs currently playing." + $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}Stop?").ConfigureAwait(false);
        }
        else
        {
          try
          {
            /* Save the current song for use after we skip it. */
            var currentTrack = player.Track;
            /* Skip the current song. */
            await player.SkipAsync().ConfigureAwait(false);
            await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}").ConfigureAwait(false);
            return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue).ConfigureAwait(false);
          }
          catch (Exception ex)
          {
            return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message).ConfigureAwait(false);
          }
        }
      }
      catch (Exception ex)
      {
        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message).ConfigureAwait(false);
      }
    }

    /*This is ran when a user uses the command Stop 
        Task Returns an Embed which is used in the command call. */
    public async Task<Embed> StopAsync(IGuild guild)
    {
      try
      {
        var player = _lavaNode.GetPlayer(guild);

        if (player == null)
          return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.").ConfigureAwait(false);

        /* Check if the player exists, if it does, check if it is playing.
             If it is playing, we can stop.*/
        if (player.PlayerState is PlayerState.Playing)
        {
          await player.StopAsync().ConfigureAwait(false);
        }

        await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.").ConfigureAwait(false);
        return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message).ConfigureAwait(false);
      }
    }

    /*This is ran when a user uses the command Volume 
        Task Returns a String which is used in the command call. */
    public async Task<string> SetVolumeAsync(IGuild guild, int volume)
    {
      if (volume > 150 || volume <= 0)
      {
        return $"Volume must be between 1 and 150.";
      }
      try
      {
        var player = _lavaNode.GetPlayer(guild);
        await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
        await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}").ConfigureAwait(false);
        return $"Volume has been set to {volume}.";
      }
      catch (InvalidOperationException ex)
      {
        return ex.Message;
      }
    }

    public async Task<string> PauseAsync(IGuild guild)
    {
      try
      {
        var player = _lavaNode.GetPlayer(guild);
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

    public async Task<string> ResumeAsync(IGuild guild)
    {
      try
      {
        var player = _lavaNode.GetPlayer(guild);

        if (player.PlayerState is PlayerState.Paused)
        {
          await player.ResumeAsync();
        }

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
      await args.Player.TextChannel.SendMessageAsync(
          embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue).ConfigureAwait(false)).ConfigureAwait(false);
    }
  }
}
