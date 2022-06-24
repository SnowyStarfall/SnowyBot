using SnowyBot.Services;
using Victoria.Filters;

namespace SnowyBot
{
	public static partial class Utilities
	{
		public static void ConfigureEQ()
		{
			DiscordGlobal.lavaModule.normalEQ = (new[]
{
				new EqualizerBand(0, 0),
				new EqualizerBand(1, 0),
				new EqualizerBand(2, 0),
				new EqualizerBand(3, 0),
				new EqualizerBand(4, 0),
				new EqualizerBand(5, 0),
				new EqualizerBand(6, 0),
				new EqualizerBand(7, 0),
				new EqualizerBand(8, 0),
				new EqualizerBand(9, 0),
				new EqualizerBand(10, 0),
				new EqualizerBand(11, 0),
				new EqualizerBand(12, 0),
				new EqualizerBand(13, 0),
				new EqualizerBand(14, 0)
			});
			DiscordGlobal.lavaModule.bassBoostEQ = (new[]
			{
				new EqualizerBand(0, 0.99),
				new EqualizerBand(1, 0.99),
				new EqualizerBand(2, 0.99),
				new EqualizerBand(3, 0.99),
				new EqualizerBand(4, 0.99),
				new EqualizerBand(5, 0.99),
				new EqualizerBand(6, 0.99),
				new EqualizerBand(7, 0.99),
				new EqualizerBand(8, 0.99),
				new EqualizerBand(9, 0.99),
				new EqualizerBand(10, 0.99),
				new EqualizerBand(11, 0.99),
				new EqualizerBand(12, 0.99),
				new EqualizerBand(13, 0.99),
				new EqualizerBand(14, 0.99)
			});
		}
	}
}
