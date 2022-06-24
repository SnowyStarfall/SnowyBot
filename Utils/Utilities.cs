using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SnowyBot
{
	public static partial class Utilities
	{
		public enum ExceptionReason
		{
			Unknown,
			MissingPermisions,
		}
		public static ExceptionReason InterpretException(Exception ex)
		{
			ExceptionReason reason = ExceptionReason.Unknown;

			if (ex.ToString().Contains("Missing Permissions"))
				reason = ExceptionReason.MissingPermisions;

			Console.WriteLine("Unknown Error:\n" + ex);
			return reason;
		}
		public static async Task TryDeleteAsync(this IUserMessage message)
		{
			try { await message.DeleteAsync().ConfigureAwait(false); }
			catch { }
		}
		public static async Task TimedMessage(IUserMessage message, int time, bool shouldDelete)
		{
			if (shouldDelete)
			{
				await Task.Delay(time).ConfigureAwait(false);
				try { await message.DeleteAsync().ConfigureAwait(false); }
				catch { }
			}
		}
	}
}
