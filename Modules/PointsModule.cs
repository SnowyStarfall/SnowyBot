using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using SnowyBot.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
	public class PointsModule : ModuleBase
	{
		public readonly Guilds guilds;
		public readonly Random random = new();
		public PointsModule(Guilds guilds)
		{
			this.guilds = guilds;
		}

		// Commands
		[Command("leaderboard")]
		public async Task Leaderboard()
		{
			string result = "";
			List<string> leaderboard = await guilds.GetPointsLeaderboard(Context.Guild.Id).ConfigureAwait(false);
			SocketUser user;
			int place = 1;
			foreach (string s in leaderboard)
			{
				if (s?.Length == 0)
					continue;
				string[] s2 = s.Split(';');
				user = DiscordGlobal.client.GetUser(ulong.Parse(s2[0]));
				if (user == null)
					result += ($"{Utilities.NumToDarkEmoji(place)} Invalid-user {Utilities.SnowySmallButton} `{s2[1]}` points. {Utilities.SnowyUniversalStrong}\n");
				else
					result += ($"{Utilities.NumToDarkEmoji(place)} {user.Mention} {Utilities.SnowySmallButton} `{s2[1]}` points. {Utilities.SnowyUniversalStrong}\n");
				place++;
				if (place > 10)
					break;
			}
			EmbedBuilder builder = new();
			builder.WithThumbnailUrl(Context.Guild.IconUrl);
			builder.WithTitle($"Leaderboard for {Context.Guild.Name}!");
			builder.WithFooter("Bot made by SnowyStarfall - Snowy#8364", DiscordGlobal.Snowy.GetAvatarUrl(ImageFormat.Png));
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(result);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("Points")]
		public async Task Points()
		{
			ulong amount = await guilds.GetGuildPoints(Context.Guild.Id, Context.User.Id).ConfigureAwait(false);
			await Context.Channel.SendMessageAsync("You have " + amount + " points.").ConfigureAwait(false);
		}
		[Command("Leaderboard Remove")]
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

		// Responses
		public async Task AddPoints(SocketCommandContext context)
		{
			(int min, int max) = await guilds.GetGuildPointRange(context.Guild.Id).ConfigureAwait(false);
			ulong amount = (ulong)random.Next(min, max + 1);

			if (context.Guild != null)
			{
				PointCooldownData data = DiscordGlobal.pointCooldownData.Find(x => x.guild.Id == context.Guild.Id);
				if (DiscordGlobal.pointCooldownData.Contains(data) && data.messages < 5)
				{
					data.messages++;
					await guilds.UpdateGuildPoints(context.Guild.Id, context.User.Id, amount).ConfigureAwait(false);
				}
				else
				{
					await guilds.UpdateGuildPoints(context.Guild.Id, context.User.Id, amount).ConfigureAwait(false);
					data = new()
					{
						guild = context.Guild,
						user = context.User,
						messages = 1
					};
					DiscordGlobal.pointCooldownData.Add(data);
				}
			}
		}
	}
}
