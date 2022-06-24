using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria.Enums;
using Victoria;

namespace SnowyBot.Containers
{
	[Serializable]
	public class LavaEntry
	{
		public DateTime offlineTime;

		// LavaTrack
		public string Hash;
		public string Id;
		public string Author;
		public string Title;
		public bool CanSeek;
		public TimeSpan Duration;
		public bool IsStream;
		public TimeSpan Position;
		public string Url;
		public string Source;

		// LavaPlayer
		public int volume;
		public ulong text;
		public ulong voice;
		public PlayerState playerState;
		public List<string> queue;

		public void Configure(LavaPlayer player)
		{
			offlineTime = DateTime.Now;
			Hash = player.Track.Hash;
			Id = player.Track.Id;
			Author = player.Track.Author;
			Title = player.Track.Title;
			CanSeek = player.Track.CanSeek;
			Duration = player.Track.Duration;
			IsStream = player.Track.IsStream;
			Position = player.Track.Position;
			Url = player.Track.Url;
			Source = player.Track.Source;
			volume = player.Volume;
			if (volume == 0)
				volume = 75;
			text = player.TextChannel.Id;
			voice = player.VoiceChannel.Id;
			playerState = player.PlayerState;
			queue = new();
			foreach (LavaTrack track in player.Queue)
			{
				if (track == null)
					continue;
				string s = track.Hash + ";" + track.Id + ";" + track.Title + ";" + track.Author + ";" + track.Url + ";" + track.Position + ";" + track.Duration + ";" + track.CanSeek + ";" + track.IsStream + ";" + track.Source;
				queue.Add(s);
			}
		}
	}
}
