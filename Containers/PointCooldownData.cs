using Discord;

namespace SnowyBot.Structs
{
	public class PointCooldownData
	{
		public IGuild guild;
		public IUser user;
		public int messages;
		public int timer;
	}
}
