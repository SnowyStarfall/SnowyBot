using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Interactivity;
using SnowyBot.Database;
using SnowyBot.Services;
using SnowyBot.Structs;
using SnowyBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Discord.MentionUtils;

namespace SnowyBot.Handlers
{
	public class CommandHandler
	{
		private static IServiceProvider provider;
		private static DiscordSocketClient client;
		private static CommandService commands;
		private static Guilds guilds;
		private static Characters characters;
		private static readonly Random random = new();
		public int playerCount;
		public CommandHandler(DiscordSocketClient _client, CommandService _commands, IServiceProvider _provider, Guilds _guilds, Characters _characters)
		{
			provider = _provider;
			client = _client;
			commands = _commands;
			guilds = _guilds;
			characters = _characters;

			client.MessageReceived += Client_MessageRecieved;
			client.InteractionCreated += Client_InteractionCreated;
			client.ReactionAdded += Client_ReactionAdded;
			client.ReactionRemoved += Client_ReactionRemoved;
			client.MessageUpdated += Client_MessageUpdated;
		}

		private async Task Client_MessageRecieved(SocketMessage arg)
		{
			SocketUserMessage socketMessage = arg as SocketUserMessage;
			if (socketMessage == null)
				return;
			SocketCommandContext context = new(client, socketMessage);

			if (arg is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
				return;

			string[] array = context.Message.Content.Split(" ");
			string prefix = "";
			try
			{
				prefix = await guilds.GetGuildPrefix(context.Guild.Id).ConfigureAwait(false) ?? "!";
			}
			catch (Exception ex)
			{
				Console.Write(ex.ToString());
			}
			int argPos = 0;

			(Character character, string charPrefix) = await characters.HasCharPrefix(context.User.Id, context.Message.Content).ConfigureAwait(false);

			if (character != null)
			{
				try { await context.Message.DeleteAsync().ConfigureAwait(false); }
				catch (Exception) { }

				try
				{
					var webhooks = await context.Guild.GetWebhooksAsync().ConfigureAwait(false);
					string url = null;
					foreach (RestWebhook webhook in webhooks)
					{
						if (string.Equals(webhook.Name, "snowybot", StringComparison.OrdinalIgnoreCase) && webhook.ChannelId == context.Channel.Id)
						{
							url = $"https://ptb.discord.com/api/webhooks/{webhook.Id}/{webhook.Token}";
							break;
						}
					}
					if (url == null) return;
					DiscordWebhookClient webClient = new(url);
					ulong messageID = await webClient.SendMessageAsync(context.Message.Content.Remove(0, charPrefix.Length), false, null, character.Name, character.AvatarURL).ConfigureAwait(false);
					IUserMessage message1 = await context.Channel.GetMessageAsync(messageID).ConfigureAwait(false) as IUserMessage;
					DiscordService.messageData.TryAdd(messageID, (message1, 600, true, context.User.Id));
				}
				catch (Exception ex)
				{
					Console.Write(ex);
					if (ex.ToString().Contains("Missing Permissions"))
						await context.Channel.SendMessageAsync("I lack the permissions to grab WebHooks. Please enable Manage Webhooks for my role.").ConfigureAwait(false);
				}
				return;
			}

			(int min, int max) = await guilds.GetGuildPointRange(context.Guild.Id).ConfigureAwait(false);
			ulong amount = (ulong)random.Next(min, max + 1);

			if (context.Guild != null)
			{
				if (DiscordService.pointCooldownData.ContainsKey(await guilds.GetGuild(context.Guild.Id).ConfigureAwait(false)))
				{
					DiscordService.pointCooldownData.TryGetValue(await guilds.GetGuild(context.Guild.Id).ConfigureAwait(false), out List<(ulong userID, int messages, int timer)> values);
					(ulong userID, int messages, int timer) tempValue = (0, 0, 0);
					(ulong userID, int messages, int timer) resultValue = (0, 0, 0);

					foreach (var item in values)
						if (item.userID == context.User.Id)
						{
							tempValue = item;
							resultValue = tempValue;
						}
					if (resultValue.userID != 0)
					{
						if (resultValue.messages != 5)
							await guilds.UpdateGuildPoints(context.Guild.Id, context.User.Id, amount).ConfigureAwait(false);
						if (resultValue.messages < 5)
						{
							resultValue.messages++;
							int index = values.IndexOf(tempValue);
							values[index] = resultValue;
						}
					}
					else
					{
						await guilds.UpdateGuildPoints(context.Guild.Id, context.User.Id, amount).ConfigureAwait(false);
						values.Add((context.User.Id, 1, 5));
					}
				}
				else
				{
					List<(ulong userID, int messages, int timer)> values = new();
					await guilds.UpdateGuildPoints(context.Guild.Id, context.User.Id, amount).ConfigureAwait(false);
					values.Add((context.User.Id, 1, 5));
					DiscordService.pointCooldownData.TryAdd(await guilds.GetGuild(context.Guild.Id).ConfigureAwait(false), values);
				}
			}

			if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(client.CurrentUser, ref argPos))
				return;

			IResult result = await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best).ConfigureAwait(false);
			//if (result.Error == CommandError.UnknownCommand)
			//  await arg.Channel.SendMessageAsync("Unknown command. You may correct your post.").ConfigureAwait(false);
			if (result.Error == CommandError.UnmetPrecondition)
				await arg.Channel.SendMessageAsync("You lack the permissions to use this command.").ConfigureAwait(false);
		}
		private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
		{
			if (DateTime.Now - arg2.Timestamp >= TimeSpan.FromMinutes(1))
				return;
			if (DiscordService.messageData.ContainsKey(arg2.Id))
			{
				await Client_MessageRecieved(arg2).ConfigureAwait(false);
				DiscordService.messageData.Remove(arg2.Id, out _);
				return;
			}
			DiscordService.messageData.TryAdd(arg2.Id, (arg2 as IUserMessage, 60, false, arg2.Author.Id));
		}
		private async Task Client_InteractionCreated(SocketInteraction interaction)
		{
			switch (interaction)
			{
				case SocketSlashCommand commandInteraction:
					await commandInteraction.DeferAsync().ConfigureAwait(false);
					await HandleSlash(commandInteraction).ConfigureAwait(false);
					break;
				case SocketMessageComponent componentInteraction:
					await componentInteraction.DeferAsync().ConfigureAwait(false);
					await HandleComponent(componentInteraction).ConfigureAwait(false);
					break;
				default:
					break;
			}
		}
		private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
		{
			try
			{
				if (DiscordService.messageData.ContainsKey(arg1.Id))
				{
					DiscordService.messageData.TryGetValue(arg1.Id, out (IUserMessage message, int timer, bool webHook, ulong author) value);
					if (value.webHook && value.author == arg3.UserId && arg3.Emote.Name == "❌")
					{
						try { await value.message.DeleteAsync().ConfigureAwait(false); }
						catch (Exception ex) { Console.Write(ex); }
						DiscordService.messageData.TryRemove(arg1.Id, out _);
					}
				}
			}
			catch (Exception ex)
			{ Console.Write(ex); }
			string roleID = await guilds.ExistsReactiveRole((arg3.Channel as SocketGuildChannel).Guild.Id, arg3.MessageId, arg3.Emote.Name).ConfigureAwait(false);
			if (roleID == null)
				return;
			SocketGuildUser user = arg3.User.GetValueOrDefault() as SocketGuildUser;
			if (user.IsBot)
				return;
			if (user.Roles.Contains(user.Guild.GetRole(ulong.Parse(roleID))))
				return;
			SocketRole role = user.Guild.GetRole(ulong.Parse(roleID));
			await user.AddRoleAsync(role).ConfigureAwait(false);
		}
		private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
		{
			string roleID = await guilds.ExistsReactiveRole((arg3.Channel as SocketGuildChannel).Guild.Id, arg3.MessageId, arg3.Emote.Name).ConfigureAwait(false);
			if (roleID == null)
				return;
			SocketGuildUser user = arg3.User.GetValueOrDefault() as SocketGuildUser;
			if (user.IsBot)
				return;
			if (!user.Roles.Contains(user.Guild.GetRole(ulong.Parse(roleID))))
				return;
			SocketRole role = user.Guild.GetRole(ulong.Parse(roleID));
			await user.RemoveRoleAsync(role).ConfigureAwait(false);
		}
		private async Task HandleSlash(SocketSlashCommand command)
		{
			Console.WriteLine("Handle slash " + new NotImplementedException());
		}
		private async Task HandleComponent(SocketMessageComponent component)
		{
			string[] data = component.Data.CustomId.Split(":");
			if (component.User.Id != ulong.Parse(data[1]))
				return;
			switch (data[0])
			{
				case "EditCharacterBack":
					await component.Message.ModifyAsync((MessageProperties properties) =>
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
					await component.Message.ModifyAsync((MessageProperties properties) =>
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
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Prefix, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterName":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Name, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterGender":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Gender, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterSex":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Sex, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterSpecies":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Species, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterAge":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Age, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterHeight":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Height, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterWeight":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Weight, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterOrientation":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Orientation, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterDescription":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.Description, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterAvatar":
					await component.Message.ModifyAsync((MessageProperties properties) =>
					{
						ComponentBuilder builder = new();
						builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
						properties.Components = builder.Build();
					}).ConfigureAwait(false);

					HandleCharacterEdits(component, CharacterDataType.AvatarURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

					break;
				case "EditCharacterReference":
					await component.Message.ModifyAsync((MessageProperties properties) =>
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
				builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
				return builder.Build();
			}
			bool timeOut = false;

			switch (type)
			{
				case CharacterDataType.Delete:
					bool parsed = ulong.TryParse(data[1], out ulong value);
					Character character2 = await characters.ViewCharacterByID(value, data[2]).ConfigureAwait(false);

					string key = null;

					for (int i = 0; i < 5; i++)
						key += random.Next(0, 10);

					await component.Channel.SendMessageAsync($"Are you sure you want to delete this character? Please type {key} to confirm.").ConfigureAwait(false);

					var keyResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == value) && (x.Channel.Id == component.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (keyResult.IsSuccess)
					{
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
					}
					else
					{
						await component.Channel.SendMessageAsync("Timed out. Cancelling...").ConfigureAwait(false);
						return;
					}
					break;
				case CharacterDataType.Prefix:
					RestUserMessage infoMessage1 = await component.Message.Channel.SendMessageAsync("Please enter a new prefix.").ConfigureAwait(false);

					var prefixResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage1 = await component.Message.Channel.SendMessageAsync($"Prefix set to {prefixResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage1.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Name:
					RestUserMessage infoMessage2 = await component.Message.Channel.SendMessageAsync("Please enter a new name.").ConfigureAwait(false);

					var nameResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage2 = await component.Message.Channel.SendMessageAsync($"Name set to {nameResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage2.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Gender:
					RestUserMessage infoMessage3 = await component.Message.Channel.SendMessageAsync("Please enter a new gender.").ConfigureAwait(false);

					var genderResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage3 = await component.Message.Channel.SendMessageAsync($"Gender set to {genderResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage3.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Sex:
					RestUserMessage infomMessage4 = await component.Message.Channel.SendMessageAsync("Please enter a new sex.").ConfigureAwait(false);

					var sexResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage4 = await component.Message.Channel.SendMessageAsync($"Sex set to {sexResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage4.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Species:
					RestUserMessage infoMessage5 = await component.Message.Channel.SendMessageAsync("Please enter a new species.").ConfigureAwait(false);

					var speciesResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage5 = await component.Message.Channel.SendMessageAsync($"Species set to {speciesResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage5.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Age:
					RestUserMessage infoMessage6 = await component.Message.Channel.SendMessageAsync("Please enter a new age.").ConfigureAwait(false);

					var ageResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage6 = await component.Message.Channel.SendMessageAsync($"Age set to {ageResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage6.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Height:
					RestUserMessage infoMessage7 = await component.Message.Channel.SendMessageAsync("Please enter a new height.").ConfigureAwait(false);

					var heightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage7 = await component.Message.Channel.SendMessageAsync($"Height set to {heightResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage7.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Weight:
					RestUserMessage infoMessage8 = await component.Message.Channel.SendMessageAsync("Please enter a new weight.").ConfigureAwait(false) as RestUserMessage;

					var weightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage8 = await component.Message.Channel.SendMessageAsync($"Weight set to {weightResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage8.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Orientation:
					RestUserMessage infoMessage9 = await component.Message.Channel.SendMessageAsync("Please enter a new orientation.").ConfigureAwait(false);

					var orientationResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage9 = await component.Message.Channel.SendMessageAsync($"Orientation set to {orientationResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage9.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.Description:
					RestUserMessage infoMessage10 = await component.Message.Channel.SendMessageAsync("Please enter a new description.").ConfigureAwait(false);

					var descriptionResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

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

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage10 = await component.Message.Channel.SendMessageAsync($"Description set to {descriptionResult.Value.Content}").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage10.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.AvatarURL:
					RestUserMessage infoMessage11 = await component.Message.Channel.SendMessageAsync("Please send a new avatar.").ConfigureAwait(false);

					var avatarResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Attachments.First() != null), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (avatarResult.IsSuccess)
					{
						await infoMessage11.DeleteAsync().ConfigureAwait(false);

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.AvatarURL, avatarResult.Value.Attachments.First().Url).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage11 = await component.Message.Channel.SendMessageAsync($"Avatar set.").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage11.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
					break;
				case CharacterDataType.ReferenceURL:
					RestUserMessage infoMessage12 = await component.Message.Channel.SendMessageAsync("Please send a new reference.").ConfigureAwait(false);

					var referenceResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Attachments.Count > 0) && (x.Attachments.First() != null), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

					if (referenceResult.IsSuccess)
					{
						await infoMessage12.DeleteAsync().ConfigureAwait(false);

						await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.ReferenceURL, referenceResult.Value.Attachments.First().Url).ConfigureAwait(false);
						Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

						ComponentBuilder builder = new();
						builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
						builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

						await component.Message.ModifyAsync((MessageProperties properties) =>
						{
							properties.Embeds = new Embed[1] { CreateCharEmbed(character) };
							properties.Components = builder.Build();
						}).ConfigureAwait(false);

						RestUserMessage notifMessage12 = await component.Message.Channel.SendMessageAsync($"Reference set.").ConfigureAwait(false);
						await Task.Delay(5000).ConfigureAwait(false);
						await notifMessage12.DeleteAsync().ConfigureAwait(false);
					}
					else
					{
						timeOut = true;
					}
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

			IGuild iGuild = DiscordService.client.GetGuild(guildID);
			Guild guild = await guilds.GetGuild(guildID).ConfigureAwait(false);
			SocketTextChannel channel = await iGuild.GetChannelAsync(channelID).ConfigureAwait(false) as SocketTextChannel;
			IUserMessage message = await channel.GetMessageAsync(messageID).ConfigureAwait(false) as IUserMessage;

			ulong role = 0;
			RestUserMessage m1 = await component.Channel.SendMessageAsync($"Send the role to {(data[0] == "ReactiveRolesAdd" ? "link to" : "unlink from")} the message.").ConfigureAwait(false);
			InteractivityResult<SocketMessage> r1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == userID) && (x.Channel.Id == component.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
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
			RestUserMessage m4 = await component.Channel.SendMessageAsync($"Send the emoji linked to that role. (Do not use emojis from a server that I'm not in.)").ConfigureAwait(false);
			var e1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == component.User.Id) && (x.Channel.Id == component.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
			if (e1.IsSuccess)
			{
				flag1 = Emoji.TryParse(e1.Value.Content.ToString(), out Emoji emojiID1);
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
					await message.RemoveReactionAsync(flag1 ? emojiResult : emoteResult, DiscordService.client.CurrentUser).ConfigureAwait(false);
					await guilds.RemoveReactiveRole(guildID, channelID, messageID, role, emoji).ConfigureAwait(false);
					await Task.Delay(5000).ConfigureAwait(false);
					await m8.DeleteAsync().ConfigureAwait(false);
					break;
			}
		}
		private async Task HandlePaginators(SocketMessageComponent component, string[] data)
		{
			bool found = DiscordService.paginators.TryGetValue(component.Message.Id, out (Paginator paginator, int timer) value);
			if (!found)
				return;
			switch (data[0])
			{
				case "NextPage":
					await value.paginator.NextPage().ConfigureAwait(false);
					break;
				case "PreviousPage":
					await value.paginator.PreviousPage().ConfigureAwait(false);
					break;
				case "NextPageThree":
					await value.paginator.Forward3Pages().ConfigureAwait(false);
					break;
				case "PreviousPageThree":
					await value.paginator.Backward3Pages().ConfigureAwait(false);
					break;
			}
		}
	}
}
