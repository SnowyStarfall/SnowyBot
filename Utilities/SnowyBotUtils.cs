using Discord;
using System.Threading.Tasks;

namespace SnowyBot
{
	public static partial class SnowyBotUtils
	{
		public static async Task TimedMessage(IUserMessage message, int time, bool shouldDelete)
		{
			if (shouldDelete)
			{
				await Task.Delay(time).ConfigureAwait(false);
				await message.DeleteAsync().ConfigureAwait(false);
			}
		}
	}
}
