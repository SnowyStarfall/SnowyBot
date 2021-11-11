using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Interactivity;
using SnowyBot.Database;
using SnowyBot.Handlers;
using SnowyBot.Services;
using SnowyBot.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
  public class CommandHandler
  {
    private static IServiceProvider provider;
    private static DiscordSocketClient client;
    private static CommandService commands;
    private static Guilds guilds;
    private static Characters characters;
    private static Random random = new Random();
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
    }

    private async Task Client_MessageRecieved(SocketMessage arg)
    {
      var socketMessage = arg as SocketUserMessage;

      if (!(arg is SocketUserMessage message) || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
        return;

      var context = new SocketCommandContext(client, socketMessage);
      string prefix = null;
      string[] array = context.Message.Content.Split(" ");
      try
      {
        prefix = await guilds.GetGuildPrefix(context.Guild.Id).ConfigureAwait(false) ?? "!";
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      var argPos = 0;

      Character character = await characters.HasCharPrefix(context.User.Id, context.Message.Content).ConfigureAwait(false);

      if (character != null)
      {
        try { await context.Message.DeleteAsync().ConfigureAwait(false); }
        catch (Exception) { }

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
        DiscordWebhookClient webClient = new DiscordWebhookClient(url);
        await webClient.SendMessageAsync(context.Message.Content.Remove(0, array[0].Length + 1), false, null, character.Name, character.AvatarURL).ConfigureAwait(false);
        return;
      }

      if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(DiscordService.client.CurrentUser, ref argPos))
        return;

      await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best).ConfigureAwait(false);
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
      Console.WriteLine("Reaction added " + new NotImplementedException());
    }
    private async Task HandleSlash(SocketSlashCommand command)
    {
      Console.WriteLine("Handle slash " + new NotImplementedException());
    }
    private async Task HandleComponent(SocketMessageComponent component)
    {
      // This needs updating.
      // data[0] = User ID
      // data[1] = Character ID
      // data[2] = Channel ID
      string[] data = component.Data.CustomId.Split(":");
      if (component.User.Id != ulong.Parse(data[1]))
        return;
      switch (data[0])
      {
        case "EditCharacterBack":
          Character character = await characters.ViewCharacterByID(ulong.Parse(data[0]), data[1]).ConfigureAwait(false);

          string key = null;

          for (int i = 0; i < 10; i++)
            key += random.Next(0, 10);

          await component.Channel.SendMessageAsync($"Are you sure you want to delete this character? Please type {key} to confirm.").ConfigureAwait(false);

          var keyResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == component.User.Id) && (x.Channel.Id == component.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (keyResult.IsSuccess)
          {
            if (keyResult.Value.Content == key)
            {
              await characters.DeleteCharacter(character).ConfigureAwait(false);
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
        case "DeleteCharacter":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Edit", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Delete", $"DeleteCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);
          break;
        case "EditCharacter":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
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
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Prefix, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterName":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Name, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterGender":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Gender, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterSex":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Sex, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterSpecies":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Species, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterAge":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Age, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterHeight":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Height, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterWeight":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Weight, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterOrientation":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Orientation, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterDescription":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Description, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterAvatar":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.AvatarURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterReference":
          await component.Message.ModifyAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.ReferenceURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
      }
    }
    private async Task HandleCharacterEdits(SocketMessageComponent component, CharacterDataType type, string[] data)
    {
      switch (type)
      {
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage1 = await component.Message.Channel.SendMessageAsync($"Prefix set to {prefixResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage1.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage1 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage1.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage2 = await component.Message.Channel.SendMessageAsync($"Name set to {nameResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage2.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage2 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage2.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage3 = await component.Message.Channel.SendMessageAsync($"Gender set to {genderResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage3.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage3 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage3.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage4 = await component.Message.Channel.SendMessageAsync($"Sex set to {sexResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage4.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage4 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage4.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage5 = await component.Message.Channel.SendMessageAsync($"Species set to {speciesResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage5.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage5 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage5.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage6 = await component.Message.Channel.SendMessageAsync($"Age set to {ageResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage6.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage6 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage6.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage7 = await component.Message.Channel.SendMessageAsync($"Height set to {heightResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage7.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage7 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage7.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage8 = await component.Message.Channel.SendMessageAsync($"Weight set to {weightResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage8.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage8 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage8.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage9 = await component.Message.Channel.SendMessageAsync($"Orientation set to {orientationResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage9.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage9 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage9.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage10 = await component.Message.Channel.SendMessageAsync($"Description set to {descriptionResult.Value.Content}").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage10.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage10 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage10.DeleteAsync().ConfigureAwait(false);
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

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage11 = await component.Message.Channel.SendMessageAsync($"Avatar set.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage11.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage11 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage11.DeleteAsync().ConfigureAwait(false);
          }
          break;
        case CharacterDataType.ReferenceURL:
          RestUserMessage infoMessage12 = await component.Message.Channel.SendMessageAsync("Please send a new reference.").ConfigureAwait(false);

          var referenceResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Attachments.First() != null), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (referenceResult.IsSuccess)
          {
            await infoMessage12.DeleteAsync().ConfigureAwait(false);

            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.ReferenceURL, referenceResult.Value.Attachments.First().Url).ConfigureAwait(false);
            Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);

            EmbedBuilder eBuilder = new EmbedBuilder();
            eBuilder.WithAuthor($"{component.User.Username}#{component.User.Discriminator}", component.User.GetAvatarUrl(ImageFormat.Gif));
            eBuilder.WithThumbnailUrl(character.AvatarURL);
            eBuilder.WithTitle(character.Name);
            eBuilder.WithDescription(character.Description);
            eBuilder.AddField("Prefix", character.Prefix, true);
            eBuilder.AddField("Gender", character.Gender, true);
            eBuilder.AddField("Sex", character.Sex, true);
            eBuilder.AddField("Species", character.Species, true);
            eBuilder.AddField("Age", character.Age + " years", true);
            eBuilder.AddField("Height", character.Height, true);
            eBuilder.AddField("Weight", character.Weight, true);
            eBuilder.AddField("Orientation", character.Orientation, true);
            eBuilder.AddField("Created", character.CreationDate, true);
            if (character.ReferenceURL != string.Empty && character.ReferenceURL != null && character.ReferenceURL != "X")
              eBuilder.WithImageUrl(character.ReferenceURL);
            eBuilder.WithCurrentTimestamp();
            eBuilder.WithColor(new Color(0xcc70ff));
            eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));

            await component.Message.ModifyAsync(async (MessageProperties properties) =>
            {
              properties.Embeds = new Embed[1] { eBuilder.Build() };
              properties.Components = builder.Build();
            }).ConfigureAwait(false);

            RestUserMessage notifMessage12 = await component.Message.Channel.SendMessageAsync($"Reference set.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await notifMessage12.DeleteAsync().ConfigureAwait(false);
          }
          else
          {
            RestUserMessage timeoutMessage12 = await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await timeoutMessage12.DeleteAsync().ConfigureAwait(false);
          }
          break;
      }
    }
  }
}
