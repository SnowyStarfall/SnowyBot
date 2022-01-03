using Discord;
using Discord.Commands;
using Discord.Webhook;
using SnowyBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;

namespace SnowyBot.Modules
{
  public class DevModule : ModuleBase
  {
    [Command("playlist")]
    [RequireOwner]
    public async Task Playlist([Remainder] string query)
    {
      List<PlaylistVideo> videos = await DiscordService.playlists.GetPlaylistResults(query).ConfigureAwait(false);
    }
  }
}
