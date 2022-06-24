using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Database
{
	public class Guilds
	{
		private readonly GuildContext context;

		public Guilds(GuildContext _context) => context = _context;
		// Returns a guild //
		public async Task<Guild> GetGuild(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!" });
			return guild;
		}
		// Prefix //
		public async Task<string> GetGuildPrefix(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
			{
				context.Add(new Guild { ID = id, Prefix = "!" });
			}
			else if (guild.Prefix?.Length == 0)
			{
				guild.Prefix = "!";
				await context.SaveChangesAsync().ConfigureAwait(false);
			}

			var prefix = await context.Guilds
				.AsAsyncEnumerable()
				.Where(x => x.ID == id)
				.Select(x => x.Prefix)
				.FirstOrDefaultAsync()
				.ConfigureAwait(false);

			return await Task.FromResult(prefix).ConfigureAwait(false);
		}
		public async Task ModifyGuildPrefix(ulong id, string prefix)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = prefix });
			else
				guild.Prefix = prefix;

			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		// Reactive Roles //
		public async Task AddReactiveRole(ulong guildID, ulong channelID, ulong messageID, ulong roleID, string emote)
		{
			var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
			if (guild == null)
			{
				context.Add(new Guild { ID = guildID, Prefix = "!", Roles = $"{channelID};{messageID};{roleID};{emote}" });
				return;
			}
			if (guild.Roles?.Length == 0 || guild.Roles == null)
				guild.Roles = $"{channelID};{messageID};{roleID};{emote}";
			else
				guild.Roles += $"|{channelID};{messageID};{roleID};{emote}";
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task RemoveReactiveRole(ulong guildID, ulong channelID, ulong messageID, ulong roleID, string emote)
		{
			var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = guildID, Prefix = "!" });
			if (!guild.Roles.Contains($"{channelID};{messageID};{roleID};{emote}"))
				return;
			int index = guild.Roles.IndexOf($"{channelID};{messageID};{roleID};{emote}");
			if (index == 0)
				guild.Roles = guild.Roles.Replace($"{channelID};{messageID};{roleID};{emote}", "");
			else
				guild.Roles = guild.Roles.Replace($"|{channelID};{messageID};{roleID};{emote}", "");
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<string> ExistsReactiveRole(ulong guildID, ulong messageID, string emote)
		{
			var guild = await context.Guilds.FindAsync(guildID).ConfigureAwait(false);
			if (guild == null)
			{
				context.Add(new Guild { ID = guildID, Prefix = "!" });
				return null;
			}
			if (guild.Roles == null)
				guild.Roles = "";
			string[] split = guild.Roles.Split('|');
			foreach (string s in split)
			{
				if (s.Contains(messageID.ToString()) && s.Contains(emote))
				{
					string[] split2 = s.Split(";");
					return split2[2];
				}
			}
			return null;
		}
		// Delete Music //
		public async Task DeleteMusic(ulong id, ICommandContext commandContext)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!" });
			guild.DeleteMusic = !guild.DeleteMusic;
			string response = guild.DeleteMusic ? "Music posts will be deleted." : "Music posts will not be deleted.";
			await commandContext.Channel.SendMessageAsync(response).ConfigureAwait(false);
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		// Welcome //
		public async Task CreateWelcomeMessage(ulong id, ulong channelID, string message)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!" });
			guild.WelcomeMessage = channelID.ToString() + ";" + message;
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<string> GetWelcomeMessage(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			return guild.WelcomeMessage;
		}
		// Goodbye //
		public async Task CreateGoodbyeMessage(ulong id, ulong channelID, string message)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!" });
			guild.GoodbyeMessage = channelID.ToString() + ";" + message;
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<string> GetGoodbyeMessage(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			return guild.GoodbyeMessage;
		}
		// XP Management //
		public async Task UpdateGuildPoints(ulong id, ulong userID, ulong amount)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!", PointGain = "5;10" });
			if (guild.UserPoints == null)
				guild.UserPoints = "";
			int index1 = guild.UserPoints.IndexOf(userID.ToString());
			if (index1 != -1)
			{
				int index2 = guild.UserPoints.IndexOf("|", index1);
				string[] split = guild.UserPoints[index1..index2].Replace("|", "").Split(";");
				string newValue = $"{userID};{ulong.Parse(split[1]) + amount}";
				guild.UserPoints = guild.UserPoints.Replace($"{split[0]};{split[1]}", newValue);
				await context.SaveChangesAsync().ConfigureAwait(false);
				return;
			}
			guild.UserPoints += $"{userID};{amount}|";
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<ulong> GetGuildPoints(ulong id, ulong userID)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
			{
				context.Add(new Guild { ID = id, Prefix = "!", PointGain = "5;10" });
				return 0;
			}
			if (!guild.UserPoints.Contains(userID.ToString()))
				return 0;
			int index1 = guild.UserPoints.IndexOf(userID.ToString());
			int index2 = guild.UserPoints.IndexOf("|", index1);
			string[] split = guild.UserPoints[index1..index2].Replace("|", "").Split(";");
			return ulong.Parse(split[1]);
		}
		public async Task<bool> DeleteGuildPoints(ulong id, ulong userID)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
			{
				context.Add(new Guild { ID = id, Prefix = "!", PointGain = "5;10" });
				return false;
			}
			if (!guild.UserPoints.Contains(userID.ToString()))
				return false;
			int index1 = guild.UserPoints.IndexOf(userID.ToString());
			int index2 = guild.UserPoints.IndexOf("|", index1);
			guild.UserPoints = guild.UserPoints.Remove(index1, index2 - index1);
			await context.SaveChangesAsync().ConfigureAwait(false);
			return true;
		}
		public async Task SetGuildPointRange(ulong id, int min, int max)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!" });
			guild.PointGain = $"{min};{max}";
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<(int min, int max)> GetGuildPointRange(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!", PointGain = "5;10" });
			if (guild.PointGain == null)
				guild.PointGain = "5;10";
			string[] split = guild.PointGain.Split(';');
			await context.SaveChangesAsync().ConfigureAwait(false);
			return (int.Parse(split[0]), int.Parse(split[1]));
		}
		public async Task<List<string>> GetPointsLeaderboard(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!", PointGain = "5;10" });
			if (guild.PointGain == null)
				guild.PointGain = "5;10";
			List<string> leaderboard = new();
			string[] split = guild.UserPoints.Split('|');
			leaderboard.AddRange(split);
			static int SortPointsList(string x, string y)
			{
				string[] split1 = x.Split(';');
				string[] split2 = y.Split(';');
				if (split1[0]?.Length == 0)
					return -1;
				if (split2[0]?.Length == 0)
					return 1;
				if (split1[0]?.Length == 0 && split2[0]?.Length == 0)
					return 0;
				ulong points1 = ulong.Parse(split1[1]);
				ulong points2 = ulong.Parse(split2[1]);
				return points1 < points2 ? -1 : points1 > points2 ? 1 : 0;
			}
			leaderboard.Sort(SortPointsList);
			leaderboard.Reverse();
			await context.SaveChangesAsync().ConfigureAwait(false);
			return leaderboard;
		}
		// Changelog //
		public async Task<ulong> GetChangelogChannel(ulong id)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!", ChangelogID = 0 });
			await context.SaveChangesAsync().ConfigureAwait(false);
			return guild.ChangelogID;
		}
		public async Task SetChangelogChannel(ulong id, ulong channelID)
		{
			var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null)
				context.Add(new Guild { ID = id, Prefix = "!", ChangelogID = 0 });
			guild.ChangelogID = channelID;
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task SendChangelogUpdate(List<ITextChannel> channels, Embed embed)
		{
			foreach (Guild guild in context.Guilds)
			{
				if (guild.ChangelogID == 0)
					continue;
				ITextChannel channel = channels.Find(x => x.Id == guild.ChangelogID);
				if (channel == null)
					continue;
				try
				{
					await channel.SendMessageAsync(null, false, embed);
				}
				catch (Exception ex)
				{
					Console.Write(ex);
				}
			}
		}
	}
}
