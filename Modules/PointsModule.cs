using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
	public class PointsModule : ModuleBase
	{
		public readonly Guilds guilds;
		public PointsModule(Guilds _guilds) => guilds = _guilds;

		[Command("leaderboard")]
		[Alias(new[] { "leader", "lb" })]
		public async Task Leaderboard()
		{
			string result = "";
			List<string> leaderboard = await guilds.GetPointsLeaderboard(Context.Guild.Id).ConfigureAwait(false);
			SocketUser user;
			int place = 1;
			foreach (string s in leaderboard)
			{
				if (s == string.Empty)
					continue;
				string[] s2 = s.Split(';');
				user = DiscordService.client.GetUser(ulong.Parse(s2[0]));
				if (user == null)
					result += ($"{SnowyBotUtils.NumToDarkEmoji(place)} Invalid-user {SnowyBotUtils.SnowySmallButton} `{s2[1]}` points. {SnowyBotUtils.SnowyUniversalStrong}\n");
				else
					result += ($"{SnowyBotUtils.NumToDarkEmoji(place)} {user.Mention} {SnowyBotUtils.SnowySmallButton} `{s2[1]}` points. {SnowyBotUtils.SnowyUniversalStrong}\n");
				place++;
				if (place > 10)
					break;
			}
			EmbedBuilder builder = new();
			builder.WithThumbnailUrl(Context.Guild.IconUrl);
			builder.WithTitle($"Leaderboard for {Context.Guild.Name}!");
			builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(result);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("Points")]
		public async Task Points([Remainder] string message = null)
		{
			ulong amount = await guilds.GetGuildPoints(Context.Guild.Id, Context.User.Id).ConfigureAwait(false);
			await Context.Channel.SendMessageAsync("You have " + amount + " points.").ConfigureAwait(false);
		}
		[Command("Leaderboard Remove")]
		[Alias(new[] { "lbr", "lb r", "lb remove" })]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task LeaderboardRemove(string user)
		{
			if (MentionUtils.TryParseUser(user, out ulong ID))
			{
				bool complete = await guilds.DeleteGuildPoints(Context.Guild.Id, ID).ConfigureAwait(false);
				if (!complete)
					Context.Channel.SendMessageAsync("User does not exist on the leaderboard.");
				else
					Context.Channel.SendMessageAsync("User removed from the leaderboard.");
				return;
			}
			if (ulong.TryParse(user, out ulong ID1))
			{	
				bool complete = await guilds.DeleteGuildPoints(Context.Guild.Id, ID1).ConfigureAwait(false);
				if (!complete)
					Context.Channel.SendMessageAsync("User does not exist on the leaderboard.");
				else
					Context.Channel.SendMessageAsync("User removed from the leaderboard.");
				return;
			}
		}
	}
}
