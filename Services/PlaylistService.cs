using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace SnowyBot.Services
{
  public class PlaylistService
  {
    private static YoutubeClient client;
    public PlaylistService(YoutubeClient _client) => client = _client;

    public async Task<List<PlaylistVideo>> GetPlaylistResults(string query)
    {
      string playlistID = PlaylistId.Parse(query);

      return await client.Playlists.GetVideosAsync(playlistID).ToListAsync().ConfigureAwait(false);
    }
  }
}
