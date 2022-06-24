using Discord;
using Victoria;

namespace SnowyBot.Structs
{
	public class LavaData
	{
		public LavaPlayer player;
		public LavaTrack track;
		public int loop;
		public int timer;
		public IGuild Guild { get => player.TextChannel.Guild; }
	}
}
