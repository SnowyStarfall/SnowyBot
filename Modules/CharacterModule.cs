using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using SnowyBot.Services;
using SnowyBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SnowyBot.SnowyBotUtils;

namespace SnowyBot.Modules
{
	public class CharacterModule : ModuleBase
	{
		public Random random = new();
		public readonly Characters characters;
		public CharacterModule(Characters _characters) => characters = _characters;

		[Command("cha")]
		[Alias(new string[] { "character add", "character create", "char add", "char create" })]
		public async Task AddCharacter()
		{
			//bool found = DiscordService.activeCommands.TryGetValue(Context.User.Id, out string commands);
			//if(found)
			//{
			//  if (commands.Contains("AddCharacter"))
			//  {
			//    await Context.Channel.SendMessageAsync("You're already using this command.")
			//  }
			//}
			//DiscordService.activeCommands.TryAdd(Context.User.Id, )

			string prefix = null;
			string name = null;
			string gender = null;
			string sex = null;
			string species = null;
			string age = null;
			string height = null;
			string weight = null;
			string orientation = null;
			string description = null;
			string avatarURL = null;
			string referenceURL = null;

			await Context.Channel.SendMessageAsync("Please be sure to enter responses within **5 minutes**.\n" +
																						 "**These responses can be edited later.**").ConfigureAwait(false);
			int error = 0;

			while (error < 3 && prefix == null)
			{
				await Context.Channel.SendMessageAsync("Please enter a prefix.").ConfigureAwait(false);

				var prefixResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

				if (prefixResult.IsSuccess)
				{
					if (prefixResult.Value.Content.Length > 8)
					{
						error++;
						await Context.Channel.SendMessageAsync("Prefix too large!");
					}
					else if (await characters.CheckPrefixExists(Context.User.Id, prefixResult.Value.Content).ConfigureAwait(false) != null)
					{
						error++;
						Character chara = await characters.CheckPrefixExists(Context.User.Id, prefixResult.Value.Content).ConfigureAwait(false);
						await Context.Channel.SendMessageAsync($"Prefix already exists for {chara.Name}.");
					}
					else
					{
						error = 0;
						prefix = prefixResult.Value.Content;
					}
				}
				else
				{
					await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
					return;
				}
			}
			if (error >= 3)
			{
				await Context.Channel.SendMessageAsync("Too many invalid responses.").ConfigureAwait(false);
				return;
			}

			while (error < 3 && name == null)
			{
				await Context.Channel.SendMessageAsync("Please enter a name.").ConfigureAwait(false);

				var nameResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

				if (nameResult.IsSuccess)
				{
					if (int.TryParse(nameResult.Value.Content, out _))
					{
						error++;
						await Context.Channel.SendMessageAsync("Name cannot be a number.").ConfigureAwait(false);
					}
					else
					{
						error = 0;
						name = nameResult.Value.Content;
					}
				}
				else
				{
					await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
					return;
				}
			}
			if (error >= 3)
			{
				await Context.Channel.SendMessageAsync("Too many invalid responses.").ConfigureAwait(false);
				return;
			}

			await Context.Channel.SendMessageAsync("Please enter a gender.").ConfigureAwait(false);
			var genderResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (genderResult.IsSuccess)
			{
				gender = genderResult.Value.Content;
			}
			else
			{
				gender = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter a sex.").ConfigureAwait(false);
			var sexResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (sexResult.IsSuccess)
			{
				sex = sexResult.Value.Content;
			}
			else
			{
				sex = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter a species.").ConfigureAwait(false);
			var speciesResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (speciesResult.IsSuccess)
			{
				species = speciesResult.Value.Content;
			}
			else
			{
				species = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter an age.").ConfigureAwait(false);
			var ageResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (ageResult.IsSuccess)
			{
				age = ageResult.Value.Content;
			}
			else
			{
				age = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter a height.").ConfigureAwait(false);
			var heightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (heightResult.IsSuccess)
			{
				height = heightResult.Value.Content;
			}
			else
			{
				height = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter a weight.").ConfigureAwait(false);
			var weightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (weightResult.IsSuccess)
			{
				weight = weightResult.Value.Content;
			}
			else
			{
				weight = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter an orientation.").ConfigureAwait(false);
			var orientationResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (orientationResult.IsSuccess)
			{
				orientation = orientationResult.Value.Content;
			}
			else
			{
				orientation = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please enter a description. This can be edited later.").ConfigureAwait(false);
			var descriptionResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (descriptionResult.IsSuccess)
			{
				description = descriptionResult.Value.Content;
			}
			else
			{
				description = "Skipped";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please send an avatar picture, or enter \"Skip\" to skip.").ConfigureAwait(false);
			var avatarResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Attachments.Count > 0 || string.Equals(x.Content, "skip", StringComparison.OrdinalIgnoreCase)), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (avatarResult.IsSuccess)
			{
				if (string.Equals(avatarResult.Value.Content, "skip", StringComparison.OrdinalIgnoreCase))
					avatarURL = "";
				else
					avatarURL = avatarResult.Value.Attachments.First().Url;
			}
			else
			{
				avatarURL = "";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Please send a reference picture, or enter \"Skip\" to skip.").ConfigureAwait(false);
			var referenceResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Attachments.Count > 0 || string.Equals(x.Content, "skip", StringComparison.OrdinalIgnoreCase)), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
			if (referenceResult.IsSuccess)
			{
				if (string.Equals(referenceResult.Value.Content, "skip", StringComparison.OrdinalIgnoreCase))
					referenceURL = "";
				else
					referenceURL = referenceResult.Value.Attachments.First().Url;
			}
			else
			{
				referenceURL = "";
				await Context.Channel.SendMessageAsync("Skipping.").ConfigureAwait(false);
			}

			await Context.Channel.SendMessageAsync("Character added!").ConfigureAwait(false);

			await characters.AddCharacter(Context.User.Id, DateTime.Now, prefix, name, gender, sex, species, age, height, weight, orientation, description, avatarURL, referenceURL).ConfigureAwait(false);
		}
		[Command("chv")]
		[Alias(new string[] { "character view", "char view" })]
		public async Task ViewCharacter([Remainder] string input)
		{
			bool isID = int.TryParse(input, out int idSearch);

			Character character = !isID ? await characters.ViewCharacterByName(Context.User.Id, input).ConfigureAwait(false) : await characters.ViewCharacterByID(Context.User.Id, input).ConfigureAwait(false);

			if (character == null)
			{
				await Context.Channel.SendMessageAsync("Character not found. Are you sure you spelled the name correctly?").ConfigureAwait(false);
				return;
			}

			EmbedBuilder builder = new();
			builder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
			if (character.AvatarURL != "")
				builder.WithThumbnailUrl(character.AvatarURL);
			builder.WithTitle(character.Name);
			builder.WithDescription(character.Description);
			builder.AddField("Prefix", character.Prefix, true);
			builder.AddField("Gender", character.Gender, true);
			builder.AddField("Sex", character.Sex, true);
			builder.AddField("Species", character.Species, true);
			builder.AddField("Age", character.Age + " years", true);
			builder.AddField("Height", character.Height, true);
			builder.AddField("Weight", character.Weight, true);
			builder.AddField("Orientation", character.Orientation, true);
			builder.AddField("Created", character.CreationDate, true);
			if (character.ReferenceURL != "")
				builder.WithImageUrl(character.ReferenceURL);
			builder.WithCurrentTimestamp();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));

			string[] id = character.CharacterID.Split(":");

			ComponentBuilder cBuilder = new();
			cBuilder.WithButton("Edit", $"EditCharacter:{Context.User.Id}:{id[1]}:{Context.Channel.Id}", ButtonStyle.Primary);
			cBuilder.WithButton("Delete", $"DeleteCharacter:{Context.User.Id}:{id[1]}:{Context.Channel.Id}", ButtonStyle.Danger);

			await Context.Channel.SendMessageAsync(null, false, builder.Build(), null, null, null, cBuilder.Build()).ConfigureAwait(false);
		}
		[Command("chd")]
		[Alias(new string[] { "character delete", "char delete" })]
		public async Task DeleteCharacter([Remainder] string name)
		{
			string[] result = name.Split(" ");

			Character character = await characters.ViewCharacterByName(Context.User.Id, result[0]).ConfigureAwait(false);

			if (character == null)
			{
				await Context.Channel.SendMessageAsync("Character not found. Are you sure you spelled the name correctly?").ConfigureAwait(false);
				return;
			}

			EmbedBuilder builder = new();
			builder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
			builder.WithThumbnailUrl(character.AvatarURL);
			builder.WithTitle(character.Name);
			builder.WithDescription(character.Description);
			builder.AddField("Prefix", character.Prefix, true);
			builder.AddField("Gender", character.Gender, true);
			builder.AddField("Sex", character.Sex, true);
			builder.AddField("Species", character.Species, true);
			builder.AddField("Age", character.Age + " years", true);
			builder.AddField("Height", character.Height, true);
			builder.AddField("Weight", character.Weight, true);
			builder.AddField("Orientation", character.Orientation, true);
			builder.AddField("Created", character.CreationDate, true);
			if (character.ReferenceURL != "X")
				builder.WithImageUrl(character.ReferenceURL);
			builder.WithCurrentTimestamp();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));

			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);

			string key = null;

			for (int i = 0; i < 10; i++)
				key += random.Next(0, 10);

			await Context.Channel.SendMessageAsync($"Are you sure you want to delete this character? Please type {key} to confirm.").ConfigureAwait(false);

			var keyResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

			if (keyResult.IsSuccess)
			{
				if (keyResult.Value.Content == key)
				{
					await characters.DeleteCharacter(character).ConfigureAwait(false);
					await Context.Channel.SendMessageAsync("Character deleted. ").ConfigureAwait(false);
				}
				else
				{
					await Context.Channel.SendMessageAsync("Incorrect key. Cancelling...").ConfigureAwait(false);
					return;
				}
			}
			else
			{
				await Context.Channel.SendMessageAsync("Timed out. Cancelling...").ConfigureAwait(false);
				return;
			}
		}
		[Command("chl")]
		[Alias(new string[] { "character list", "char list" })]
		public async Task ListCharacters()
		{
			List<Character> chars = await characters.ListCharacters(Context.User.Id).ConfigureAwait(false);
			if (chars == null)
			{
				await Context.Channel.SendMessageAsync("No characters found.").ConfigureAwait(false);
				return;
			}

			chars.Sort((x, y) => string.Compare(x.Name, y.Name));
			List<Embed> embeds = new();
			int embedsNeeded = chars.Count > 10 ? (chars.Count / 10) + 1 : 1;
			int embedNum = 0;
			for (int i = 0; i < embedsNeeded; i++)
			{
				EmbedBuilder builder = new();
				builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Results {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
				builder.WithColor(new Color(0xcc70ff));
				builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
				builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
				for (int k = 0; k < 10; k++)
				{
					int index1 = (embedNum * 10) + k;
					if (index1 == chars.Count)
						break;
					Character chara = chars.ElementAt(index1);
					string emojis = StringToNumbers(index1 + 1) ?? NumToDarkEmoji(index1 + 1);
					bool flag = StringToNumbers(index1 + 1) != null;
					builder.AddField($"{emojis} {SnowySmallButton} {chara.Name}", $"**Prefix:** {chara.Prefix}{SnowySmallButton}**ID:** {chara.CharacterID.Remove(0, chara.CharacterID.IndexOf(':') + 1)}", false);
				}
				embeds.Add(builder.Build());
				embedNum++;
			}
			string[] c = new[] { $"PreviousPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"PreviousPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPage:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}", $"NextPageThree:{Context.User.Id}:{Context.Guild.Id}:{Context.Channel.Id}" };

			if (embedsNeeded > 1)
			{
				ComponentBuilder cBuilder = new();
				cBuilder.WithButton(null, c[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
				cBuilder.WithButton(null, c[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				IUserMessage message = await Context.Channel.SendMessageAsync(null, false, embeds.ElementAt(0), null, null, null, cBuilder.Build()).ConfigureAwait(false);
				Paginator page = new(embeds, message, c);
				DiscordService.paginators.TryAdd(message.Id, (page, 300));
				return;
			}
			await Context.Channel.SendMessageAsync(null, false, embeds.ElementAt(0)).ConfigureAwait(false);
		}
	}
}
