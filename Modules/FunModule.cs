using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
	public class FunModule : ModuleBase
	{
		public readonly Guilds guilds;
		public FunModule(Guilds _guilds) => guilds = _guilds;

		public Random random = new();

		[Command("Info")]
		[Alias(new string[] { "Server", "Guild" })]
		public async Task Info()
		{
			Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");

			SocketCommandContext context = Context as SocketCommandContext;
			int numUsers = 0;
			int numBots = 0;
			foreach (IGuildUser user in context.Guild.Users)
			{
				if (user.IsBot)
					numBots++;
				else
					numUsers++;
			}
			EmbedBuilder builder = new EmbedBuilder();
			builder.WithTitle($"{context.Guild.Name}");
			builder.WithThumbnailUrl(context.Guild.IconUrl);
			builder.WithDescription($"{context.Guild.Description}");
			builder.WithColor(new Color(0xcc70ff));
			builder.AddField("Prefix", $"{guilds.GetGuildPrefix(context.Guild.Id).Result ?? "!"}", false);
			builder.AddField("Owner", context.Guild.Owner.Mention, true);
			builder.AddField("Creation Date", $"{context.Guild.CreatedAt}");
			builder.AddField("Boost Level", $"{context.Guild.PremiumTier.ToString().Insert(4, " ")}", true);
			builder.AddField("Text Channels", $"{context.Guild.TextChannels.Count}", true);
			builder.AddField("Voice Channels", $"{context.Guild.VoiceChannels.Count}", true);
			builder.AddField("Users", $"{numUsers}", true);
			builder.AddField("Bots", $"{numBots}", true);
			builder.AddField("Total", $"{context.Guild.MemberCount}", true);
			builder.WithTimestamp(DateTime.UtcNow);
			builder.WithFooter("Created by SnowyStarfall - Snowy#0364", context.Client.CurrentUser.GetAvatarUrl());
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("Feedback")]
		public async Task Feedback([Remainder] string feedback)
		{
			IGuild guild = await Context.Client.GetGuildAsync(814781058037317652).ConfigureAwait(false);
			ITextChannel channel = await guild.GetChannelAsync(939797088991080478).ConfigureAwait(false) as ITextChannel;
			EmbedBuilder builder = new();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(feedback);
			builder.WithAuthor(Context.User.Username + "#" + Context.User.Discriminator + " -- " + Context.User.Id, Context.User.GetAvatarUrl());
			await channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
			await Context.Channel.SendMessageAsync("Your feedback has been given to Snowy.");
		}
		[Command("Question")]
		public async Task Question([Remainder] string question = null)
		{
			Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
			int choice = random.Next(0, 101);
			if (Context.Message.Content.Contains("love me", StringComparison.OrdinalIgnoreCase) && !Context.Message.Content.Contains("not love me", StringComparison.OrdinalIgnoreCase))
				await Context.Message.ReplyAsync("yeh").ConfigureAwait(false);
			else
				await Context.Message.ReplyAsync(choice == 0 ? "NOH" : choice >= 1 && choice <= 11 ? "yeh" : choice >= 12 && choice <= 99 ? "noh" : "why are you gae").ConfigureAwait(false);
		}
		[Command("Snort")]
		public async Task Snort()
		{
			Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
			int markdown = random.Next(0, 7);
			int size = random.Next(0, 6);

			string markdownStr = markdown == 0 ? "*" :
													 markdown == 1 ? "**" :
													 markdown == 2 ? "***" :
													 markdown == 3 ? "_" :
													 markdown == 4 ? "*_" :
													 markdown == 5 ? "**_" :
													 "***_";

			string sizeStr = size == 0 ? "snort" :
											 size == 1 ? "SNORT" :
											 size == 2 ? "sɴᴏʀᴛ" :
											 size == 3 ? "ˢⁿᵒʳᵗ" :
											 size == 4 ? "ₛₙₒᵣₜ" :
											 "ˢᴺᴼᴿᵀ";

			await Context.Channel.SendMessageAsync($"-{markdownStr}{sizeStr}{new string(markdownStr.ToCharArray().Reverse().ToArray())}-").ConfigureAwait(false);
			await Context.Message.DeleteAsync().ConfigureAwait(false);
		}
		[Command("8ball")]
		[Alias(new string[] { "8" })]
		public async Task EightBall([Remainder] string question)
		{
			Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
			int type = random.Next(0, 2);
			int response = type == 0 ? random.Next(0, 11) : type == 2 ? random.Next(11, 16) : random.Next(16, 21);
			string[] responses = new string[] { "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
																					"You may rely on it.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
																					"Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
																					"Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."};
			await Context.Message.ReplyAsync($"{responses[response]}").ConfigureAwait(false);
		}
		[Command("A")]
		[Alias(new string[] { "Screm" })]
		public async Task A([Remainder] string letter)
		{
			Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");

			int caps = random.Next(0, 2);
			int num = random.Next(10, 200);

			string reply = "";
			string alpha;

			if (!(letter[0].ToString().ToLower()[0] >= 'a' && letter[0].ToString().ToLower()[0] <= 'z') && !(letter[0].ToString().ToLower()[0] >= 'A' && letter[0].ToString().ToLower()[0] <= 'Z'))
				alpha = "A";
			else
				alpha = caps == 0 ? $"{letter[0].ToString().ToLower()}" : $"{letter[0].ToString().ToUpper()}";

			for (int i = 0; i < num; i++)
			{
				reply += alpha;
			}

			await Context.Message.ReplyAsync(reply).ConfigureAwait(false);
		}
		[Command("ratewaifu")]
		[Alias(new string[] { "Rate", "Waifu" })]
		public async Task RateWaifu([Remainder] string mention = null)
		{
			if (Context.Message.MentionedUserIds.Count > 0)
			{
				IUser user = await DiscordService.client.GetUserAsync(Context.Message.MentionedUserIds.First()).ConfigureAwait(false);

				int value = 0;

				foreach (var s in user.Username)
					value += s;

				int result = value * user.Username.Length;

				int rating = result % 11;

				await Context.Channel.SendMessageAsync($"I give {user.Mention} a {rating}/10").ConfigureAwait(false);
				return;
			}
			else
			{
				if (mention == null)
				{
					int value1 = 0;

					foreach (var s in Context.User.Username)
						value1 += s;

					int result1 = value1 * Context.User.Username.Length;

					int rating1 = result1 % 11;

					await Context.Channel.SendMessageAsync($"I give you a {rating1}/10").ConfigureAwait(false);
					return;
				}
				int value2 = 0;

				foreach (var s in mention)
					value2 += s;

				int result2 = value2 * mention.Length;

				int rating2 = result2 % 11;

				await Context.Channel.SendMessageAsync($"I give {mention} a {rating2}/10").ConfigureAwait(false);
				return;
			}
		}
		[Command("Jumbo")]
		public async Task Jumbo([Remainder] string emoji)
		{
			bool valid = Emote.TryParse(emoji, out var emote);
			if (!valid)
				return;
			await Context.Channel.SendMessageAsync(emote.Url).ConfigureAwait(false);
		}
		[Command("Awoo")]
		public async Task Awoo()
		{
			int amount = random.Next(0, 101);
			string awoo = $"{(random.Next(0, 2) == 0 ? "a" : "A")}{(random.Next(0, 2) == 0 ? "w" : "W")}";

			for (int i = 0; i < amount; i++)
				awoo += random.Next(0, 2) == 0 ? "o" : "O";

			awoo += "!";

			await Context.Channel.SendMessageAsync(awoo).ConfigureAwait(false);
		}
		[Command("Roll")]
		public async Task Roll([Remainder] string dice)
		{
			string[] values = dice.ToLower().Replace("-", "").Split('d');
			bool flag1 = int.TryParse(values[0], out int amount);
			bool flag2 = int.TryParse(values[1], out int sides);
			if (!flag1 || !flag2)
			{
				await Context.Channel.SendMessageAsync("Incorrect format.").ConfigureAwait(false);
				return;
			}
			string result = "";
			int value = 0;
			for (int i = 0; i < amount; i++)
			{
				int num = random.Next(1, sides + 1);
				result += num + ", ";
				value += num;
			}
			result = result.Replace(" 1, ", " **1**,");
			result = result.Replace($" {sides}, ", $" **{sides}**, ");
			result = result.Remove(result.Length - 2, 2);
			await Context.Channel.SendMessageAsync($"{Context.User.Mention}  :game_die:\n**Result**: {dice.Replace("-", "").ToLower()} ({result})\n**Total**: {value}");
		}
		[Command("Inflate")]
		public async Task Inflate([Remainder] string mention = null)
		{
			try { await Context.Message.DeleteAsync().ConfigureAwait(false); }
			catch { }
			await Context.Channel.SendMessageAsync($"*inflates {mention} making them big and around*").ConfigureAwait(false);
		}
		[Command("Scramble")]
		public async Task Scramble([Remainder] string sentence = null)
		{
			if (Context.Message.ReferencedMessage != null)
				sentence = Context.Message.ReferencedMessage.Content;
			if (sentence == null && Context.Message.ReferencedMessage == null)
				return;
			string[] split = sentence.Split(' ');
			List<string> list = new();
			foreach (string s in split)
				list.Add(s);
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				string value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			string result = "";
			foreach (string s in list)
				result += s + " ";
			EmbedBuilder builder = new();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(result);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("FormatGreentext")]
		public async Task FormatGreentext([Remainder] string text)
		{
			EmbedBuilder builder = new();
			text = text.Replace(">", "\n>");
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(text);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("Timestamp")]
		public async Task Timestamp([Remainder] string timestamp)
		{
			bool parsed = DateTime.TryParse(timestamp, out DateTime result);
			if (!parsed)
			{
				await Context.Channel.SendMessageAsync("Incorrect time format.");
				return;
			}
			long unix = ((DateTimeOffset)result).ToUnixTimeSeconds();
			await Context.Channel.SendMessageAsync("<t:" + unix + ":F>, Raw: \\<t:" + unix + ":F>");
		}
		[Command("Kojimafy")]
		public async Task Kojimafy([Remainder] string words = null)
		{
			if (Context.Message.ReferencedMessage != null)
				words = Context.Message.ReferencedMessage.Content;
			if (words == null && Context.Message.ReferencedMessage == null)
				return;
			List<string> wordList = words.Split(' ').ToList();
			if (wordList.Count < 1)
				return;
			if (wordList.Count % 2 != 0)
				wordList.RemoveAt(wordList.Count - 1);
			int pair = 0;
			string result = "";
			foreach (string word in wordList)
			{
				if (pair == 1)
				{
					pair = 0;
					result += " " + word + "man\n";
					continue;
				}
				pair++;
				result += word;
			}
			EmbedBuilder builder = new();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(result);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
		[Command("Longembed")]
		public async Task LongEmbed()
		{
			string result = "";
			for (int i = 0; i < 4094; i++)
				result += i == 0 ? "A\n" : i == 4093 ? "A\n" : "\n";
			EmbedBuilder builder = new();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithDescription(result);
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
			//string fieldTitle = "";
			//string fieldDesc = "";
			//for (int i = 0; i < 254; i++)
			//  fieldTitle += i == 0 ? "A\n" : i == 254 ? "A\n" : "\n";
			//for (int i = 0; i < 254; i++)
			//  fieldDesc += i == 0 ? "A\n" : i == 254 ? "A\n" : "\n";

			//try
			//{
			//  for (int i = 0; i < 7; i++)
			//  {
			//    if (i != 6)
			//    {
			//      builder.AddField(fieldTitle, "A", false);

			//      continue;
			//    }
			//    builder.AddField(fieldTitle, "A", false);
			//  }
			//}
			//catch (Exception ex)
			//{
			//  Console.Write(ex);
			//}
		}
	}
}
