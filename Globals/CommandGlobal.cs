using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Interactivity;
using SnowyBot.Database;
using SnowyBot.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Discord.MentionUtils;
using static SnowyBot.Utilities;

namespace SnowyBot.Services
{
	public class CommandGlobal
	{
		private static IServiceProvider provider;
		private static DiscordShardedClient client;
		private static CommandService commands;
		private static Guilds guilds;
		private static Characters characters;
		private static readonly Random random = new();
		public int playerCount;

		public CommandGlobal(DiscordShardedClient client, CommandService commands, IServiceProvider provider, Guilds guilds, Characters characters)
		{
			CommandGlobal.provider = provider;
			CommandGlobal.client = client;
			CommandGlobal.commands = commands;
			CommandGlobal.guilds = guilds;
			CommandGlobal.characters = characters;

			CommandGlobal.client.MessageReceived += MessageRecieved;
			CommandGlobal.client.InteractionCreated += InteractionCreated;
			CommandGlobal.client.ReactionAdded += ReactionAdded;
			CommandGlobal.client.ReactionRemoved += ReactionRemoved;
		}

		private async Task MessageRecieved(SocketMessage arg)
		{
			SocketUserMessage socketMessage = arg as SocketUserMessage;
			SocketGuildUser socketUser = arg.Author as SocketGuildUser;

			if (socketMessage == null || socketUser == null)
				return;

			if (arg is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
				return;

			DiscordSocketClient shard = client.GetShardFor(socketUser.Guild);
			SocketCommandContext context = new(shard, socketMessage);

			await DiscordGlobal.characterModule.CreateCharacterMessage(context).ConfigureAwait(false);
			await DiscordGlobal.pointsModule.AddPoints(context).ConfigureAwait(false);

			string prefix = await guilds.GetGuildPrefix(context.Guild.Id).ConfigureAwait(false) ?? "!";
			int argPos = 0;
			if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(client.CurrentUser, ref argPos))
				return;

			IResult result = await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best).ConfigureAwait(false);

			if (result.Error == CommandError.UnmetPrecondition)
				await arg.Channel.SendMessageAsync("You lack the permissions to use this command.").ConfigureAwait(false);
		}
		private async Task InteractionCreated(SocketInteraction interaction)
		{
			if (interaction is SocketMessageComponent)
			{
				await interaction.DeferAsync().ConfigureAwait(false);
				await HandleComponent(interaction as SocketMessageComponent).ConfigureAwait(false);
			}
		}
		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
		{
			await DiscordGlobal.characterModule.RemoveCharacterMessage(arg1.Value, arg3).ConfigureAwait(false);
			await DiscordGlobal.configModule.AddRole(arg3).ConfigureAwait(false);
		}
		private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
		{
			await DiscordGlobal.configModule.RemoveRole(arg3).ConfigureAwait(false);
		}
		private async Task HandleComponent(SocketMessageComponent component)
		{
			string[] data = component.Data.CustomId.Split(":");
			if (component.User.Id != ulong.Parse(data[1]))
				return;
			switch (data[0])
			{
				case "EditCharacterBack":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Edit", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Delete", $"DeleteCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);
					break;
				case "DeleteCharacter":
					HandleCharacterEdits(component, CharacterDataType.Delete, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
					break;
				case "EditCharacter":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary);
						builder.WithButton("Back", $"EditCharacterBack:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);
					break;
				case "EditCharacterPrefix":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Prefix, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterName":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Name, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterGender":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Gender, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterSex":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Sex, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterSpecies":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Species, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterAge":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Age, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterHeight":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Height, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterWeight":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Weight, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterOrientation":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Orientation, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterDescription":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Description, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterAvatar":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.AvatarURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterReference":
					await component.Message.ModifyAsync((properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.ReferenceURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case string s when s.Contains("ReactiveRoles"):
					HandleReactiveRoles(component, data).ConfigureAwait(false);
					break;
				case string s when s.Contains("Page"):
					HandlePaginators(component, data).ConfigureAwait(false);
					break;
			}
		}
		private async Task HandleCharacterEdits(SocketMessageComponent component, CharacterDataType type, string[] data)
		{
			// This needs updating.
			// data[0] = User ID
			// data[1] = Character ID
			// data[2] = Channel ID
			Embed CreateCharEmbed(Character character)
			{
				EmbedBuilder builder = new();
				builder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
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
				if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
					builder.WithImageUrl(character.ReferenceURL);
				builder.WithCurrentTimestamp();
				builder.WithColor(new Color(0xcc70ff));
				builder.WithFooter("Bot made by SnowyStarfall - Snowy#8364", DiscordGlobal.Snowy.GetAvatarUrl(ImageFormat.Png));
				return builder.Build();
			}
			bool timeOut = false;

			switch (type)
			{
				case CharacterDataType.Delete:
					bool parsed = ulong.TryParse(data[1], out ulong value);
					Character character2 = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

					string key = null;

					for (int i = 0; i < 5; i++)
						key += random.Next(0, 10);

					await component.Channel.SendMessageAsync($"Are you sure you want to delete this character? Please type {key} to confirm.").ConfigureAwait(false);

					var keyResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == value && x.Channel.Id == component.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (keyResult.IsSuccess)
						if (keyResult.Value.Content == key)
						{
							await characters.DeleteCharacter(character2).ConfigureAwait(false);
							RestUserMessage m = await component.Channel.SendMessageAsync("Character deleted. ").ConfigureAwait(false);
							await Task.Delay(5000).ConfigureAwait(false);
							await m.DeleteAsync().ConfigureAwait(false);
						}
						else
						{
							RestUserMessage m = await component.Channel.SendMessageAsync("Incorrect key. Cancelling...").ConfigureAwait(false);
							return;
						}
					else
					{
						await component.Channel.SendMessageAsync("Timed out. Cancelling...").ConfigureAwait(false);
						return;
					}
					break;
				case CharacterDataType.Prefix:
					RestUserMessage infoMessage1 = await component.Message.Channel.SendMessageAsync("Please enter a new prefix.").ConfigureAwait(false);

					var prefixResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (prefixResult.IsSuccess)
					{
						await infoMessage1.DeleteAsync().ConfigureAwait(false);
						try
						{
							await prefixResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Prefix, prefixResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage1 = await component.Message.Channel.SendMessageAsync($"Prefix set to {prefixResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage1.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Name:
					RestUserMessage infoMessage2 = await component.Message.Channel.SendMessageAsync("Please enter a new name.").ConfigureAwait(false);

					var nameResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (nameResult.IsSuccess)
					{
						await infoMessage2.DeleteAsync().ConfigureAwait(false);
						try
						{
							await nameResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Name, nameResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage2 = await component.Message.Channel.SendMessageAsync($"Name set to {nameResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage2.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Gender:
					RestUserMessage infoMessage3 = await component.Message.Channel.SendMessageAsync("Please enter a new gender.").ConfigureAwait(false);

					var genderResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (genderResult.IsSuccess)
					{
						await infoMessage3.DeleteAsync().ConfigureAwait(false);
						try
						{
							await genderResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Gender, genderResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage3 = await component.Message.Channel.SendMessageAsync($"Gender set to {genderResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage3.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Sex:
					RestUserMessage infomMessage4 = await component.Message.Channel.SendMessageAsync("Please enter a new sex.").ConfigureAwait(false);

					var sexResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (sexResult.IsSuccess)
					{
						await infomMessage4.DeleteAsync().ConfigureAwait(false);
						try
						{
							await sexResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Sex, sexResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage4 = await component.Message.Channel.SendMessageAsync($"Sex set to {sexResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage4.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Species:
					RestUserMessage infoMessage5 = await component.Message.Channel.SendMessageAsync("Please enter a new species.").ConfigureAwait(false);

					var speciesResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (speciesResult.IsSuccess)
					{
						await infoMessage5.DeleteAsync().ConfigureAwait(false);
						try
						{
							await speciesResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Species, speciesResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage5 = await component.Message.Channel.SendMessageAsync($"Species set to {speciesResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage5.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Age:
					RestUserMessage infoMessage6 = await component.Message.Channel.SendMessageAsync("Please enter a new age.").ConfigureAwait(false);

					var ageResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (ageResult.IsSuccess)
					{
						await infoMessage6.DeleteAsync().ConfigureAwait(false);
						try
						{
							await ageResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Age, ageResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage6 = await component.Message.Channel.SendMessageAsync($"Age set to {ageResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage6.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Height:
					RestUserMessage infoMessage7 = await component.Message.Channel.SendMessageAsync("Please enter a new height.").ConfigureAwait(false);

					var heightResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (heightResult.IsSuccess)
					{
						await infoMessage7.DeleteAsync().ConfigureAwait(false);
						try
						{
							await heightResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Height, heightResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage7 = await component.Message.Channel.SendMessageAsync($"Height set to {heightResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage7.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Weight:
					RestUserMessage infoMessage8 = await component.Message.Channel.SendMessageAsync("Please enter a new weight.").ConfigureAwait(false) as RestUserMessage;

					var weightResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (weightResult.IsSuccess)
					{
						await infoMessage8.DeleteAsync().ConfigureAwait(false);
						try
						{
							await weightResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Weight, weightResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage8 = await component.Message.Channel.SendMessageAsync($"Weight set to {weightResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage8.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Orientation:
					RestUserMessage infoMessage9 = await component.Message.Channel.SendMessageAsync("Please enter a new orientation.").ConfigureAwait(false);

					var orientationResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (orientationResult.IsSuccess)
					{
						await infoMessage9.DeleteAsync().ConfigureAwait(false);
						try
						{
							await orientationResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Orientation, orientationResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage9 = await component.Message.Channel.SendMessageAsync($"Orientation set to {orientationResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage9.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.Description:
					RestUserMessage infoMessage10 = await component.Message.Channel.SendMessageAsync("Please enter a new description.").ConfigureAwait(false);

					var descriptionResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (descriptionResult.IsSuccess)
					{
						await infoMessage10.DeleteAsync().ConfigureAwait(false);
						try
						{
							await descriptionResult.Value.DeleteAsync().ConfigureAwait(false);
						}
						catch { }

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Description, descriptionResult.Value.Content).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage10 = await component.Message.Channel.SendMessageAsync($"Description set to {descriptionResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage10.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.AvatarURL:
					RestUserMessage infoMessage11 = await component.Message.Channel.SendMessageAsync("Please send a new avatar.").ConfigureAwait(false);

					var avatarResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Attachments.First() != null, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (avatarResult.IsSuccess)
					{
						await infoMessage11.DeleteAsync().ConfigureAwait(false);

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.AvatarURL, avatarResult.Value.Attachments.First().Url).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage11 = await component.Message.Channel.SendMessageAsync("Avatar set.").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage11.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
				case CharacterDataType.ReferenceURL:
					RestUserMessage infoMessage12 = await component.Message.Channel.SendMessageAsync("Please send a new reference.").ConfigureAwait(false);

					var referenceResult = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == ulong.Parse(data[1]) && x.Channel.Id == component.Message.Channel.Id && x.Attachments.Count > 0 && x.Attachments.First() != null, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (referenceResult.IsSuccess)
					{
						await infoMessage12.DeleteAsync().ConfigureAwait(false);

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.ReferenceURL, referenceResult.Value.Attachments.First().Url).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage12 = await component.Message.Channel.SendMessageAsync("Reference set.").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage12.DeleteAsync().ConfigureAwait(false);
					}
					else
						timeOut = true;
					break;
			}
			if (timeOut)
			{
				RestUserMessage t = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
				await Task.Delay(5000).ConfigureAwait(false);
				await t.DeleteAsync().ConfigureAwait(false);
			}
		}
		private async Task HandleReactiveRoles(SocketMessageComponent component, string[] data)
		{
			// data[1] = User ID
			// data[2] = Guild ID
			// data[3] = Channel ID
			// data[4] = Message ID
			ulong userID = ulong.Parse(data[1]);
			ulong guildID = ulong.Parse(data[2]);
			ulong channelID = ulong.Parse(data[3]);
			ulong messageID = ulong.Parse(data[4]);

			IGuild iGuild = DiscordGlobal.client.GetGuild(guildID);
			Guild guild = await guilds.GetGuild(guildID).ConfigureAwait(false);
			SocketTextChannel channel = await iGuild.GetChannelAsync(channelID).ConfigureAwait(false) as SocketTextChannel;
			IUserMessage message = await channel.GetMessageAsync(messageID).ConfigureAwait(false) as IUserMessage;

			ulong role = 0;
			RestUserMessage m1 = await component.Channel.SendMessageAsync($"Send the role to {(data[0] == "ReactiveRolesAdd" ? "link to" : "unlink from")} the message.").ConfigureAwait(false);
			InteractivityResult<SocketMessage> r1 = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == userID && x.Channel.Id == component.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
			if (r1.IsSuccess)
			{
				if (!TryParseRole(r1.Value.Content, out ulong roleID))
				{
					RestUserMessage m2 = await component.Channel.SendMessageAsync("Please enter a valid role mention.").ConfigureAwait(false);
					await m1.DeleteAsync().ConfigureAwait(false);
					await Task.Delay(5000).ConfigureAwait(false);
					await m2.DeleteAsync().ConfigureAwait(false);
					return;
				}
				role = roleID;
			}
			else
			{
				RestUserMessage m3 = await component.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
				await m1.DeleteAsync().ConfigureAwait(false);
				await Task.Delay(5000).ConfigureAwait(false);
				await m3.DeleteAsync().ConfigureAwait(false);
				return;
			}

			await m1.DeleteAsync().ConfigureAwait(false);
			await r1.Value.DeleteAsync().ConfigureAwait(false);

			bool flag1;
			bool flag2;

			Emoji emojiResult;
			Emote emoteResult;

			string emoji = "";
			RestUserMessage m4 = await component.Channel.SendMessageAsync("Send the emoji linked to that role. (Do not use emojis from a server that I'm not in.)").ConfigureAwait(false);
			var e1 = await DiscordGlobal.interactivity.NextMessageAsync(x => x.Author.Id == component.User.Id && x.Channel.Id == component.Channel.Id && x.Content != string.Empty, null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
			if (e1.IsSuccess)
			{
				flag1 = Emoji.TryParse(e1.Value.Content, out Emoji emojiID1);
				flag2 = Emote.TryParse(e1.Value.Content, out Emote emoteID1);
				emojiResult = emojiID1;
				emoteResult = emoteID1;
				if (!flag1 && !flag2)
				{
					RestUserMessage m5 = await component.Channel.SendMessageAsync("Please enter a valid emoji.").ConfigureAwait(false);
					await m4.DeleteAsync().ConfigureAwait(false);
					await Task.Delay(5000).ConfigureAwait(false);
					await m5.DeleteAsync().ConfigureAwait(false);
					return;
				}
				if (flag1)
					emoji = emojiID1.Name;
				if (flag2)
					emoji = emoteID1.ToString();
			}
			else
			{
				RestUserMessage m6 = await component.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
				await m4.DeleteAsync().ConfigureAwait(false);
				await Task.Delay(5000).ConfigureAwait(false);
				await m6.DeleteAsync().ConfigureAwait(false);
				return;
			}

			await m4.DeleteAsync().ConfigureAwait(false);
			await e1.Value.DeleteAsync().ConfigureAwait(false);

			switch (data[0])
			{
				case string s when s == "ReactiveRolesAdd":
					RestUserMessage m7 = await component.Message.Channel.SendMessageAsync($"Emoji: {emoji}\nLinked to:{iGuild.GetRole(role).Mention}\nFor message: {message.GetJumpUrl()}").ConfigureAwait(false);
					await message.AddReactionAsync(flag1 ? emojiResult : emoteResult).ConfigureAwait(false);
					await guilds.AddReactiveRole(guildID, channelID, messageID, role, emoji).ConfigureAwait(false);
					await Task.Delay(5000).ConfigureAwait(false);
					await m7.DeleteAsync().ConfigureAwait(false);
					break;
				case string s when s == "ReactiveRolesRemove":
					RestUserMessage m8 = await component.Message.Channel.SendMessageAsync($"Emoji: {emoji}\nUnlinked from:{iGuild.GetRole(role).Mention}\nFor message: {message.GetJumpUrl()}").ConfigureAwait(false);
					await message.RemoveReactionAsync(flag1 ? emojiResult : emoteResult, DiscordGlobal.client.CurrentUser).ConfigureAwait(false);
					await guilds.RemoveReactiveRole(guildID, channelID, messageID, role, emoji).ConfigureAwait(false);
					await Task.Delay(5000).ConfigureAwait(false);
					await m8.DeleteAsync().ConfigureAwait(false);
					break;
			}
		}
		private async Task HandlePaginators(SocketMessageComponent component, string[] data)
		{
			Paginator paginator = DiscordGlobal.paginators.Find(x => x.message.Id == component.Message.Id);
			if (paginator == null)
				return;
			switch (data[0])
			{
				case "NextPage":
					await paginator.NextPage().ConfigureAwait(false);
					break;
				case "PreviousPage":
					await paginator.PreviousPage().ConfigureAwait(false);
					break;
				case "NextPageThree":
					await paginator.Forward3Pages().ConfigureAwait(false);
					break;
				case "PreviousPageThree":
					await paginator.Backward3Pages().ConfigureAwait(false);
					break;
			}
		}
	}
}
