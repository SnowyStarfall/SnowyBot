using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using System.Text.Json;

namespace SnowyBot.Structs
{
	[Serializable]
	public class LavaTable
	{
		public ConcurrentDictionary<ulong, LavaData> table = new();
		public static void WriteToBinaryFile<T>(string filePath, T objectToWrite)
		{
			using Stream stream = File.Open(filePath, FileMode.Create);
			JsonSerializerOptions options = new();
			options.IncludeFields = true;
			string json = JsonSerializer.Serialize(objectToWrite, options);
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			stream.Write(bytes);
			stream.Dispose();
			stream.Close();
		}
		public static LavaTable ReadFromBinaryFile<LavaTable>(string filePath)
		{
			byte[] bytes = File.ReadAllBytes(filePath);
			string json = Encoding.UTF8.GetString(bytes);
			JsonSerializerOptions options = new();
			options.IncludeFields = true;
			options.MaxDepth = 64;
			LavaTable table = JsonSerializer.Deserialize<LavaTable>(json, options);
			return table;
		}
	}
	[Serializable]
	public class LavaData
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
