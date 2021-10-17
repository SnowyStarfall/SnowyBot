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
    public static IServiceProvider provider;
    public static DiscordSocketClient client;
    public static CommandService commands;
    public static Guilds guilds;
    public static Characters characters;
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
          if (webhook.Name.ToLower() == "snowybot" && webhook.ChannelId == context.Channel.Id)
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

      var blacklistedChannelCheck = from a in GlobalData.Config.BlacklistedChannels
                                    where a == context.Channel.Id
                                    select a;
      var blacklistedChannel = blacklistedChannelCheck.FirstOrDefault();

      if (blacklistedChannel != context.Channel.Id)
      {
        await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best).ConfigureAwait(false);
        return;
      }
    }
    private async Task Client_InteractionCreated(SocketInteraction interaction)
    {
      switch (interaction)
      {
        case SocketSlashCommand commandInteraction:
          await HandleSlash(commandInteraction).ConfigureAwait(false);
          break;
        case SocketMessageComponent componentInteraction:
          await HandleComponent(componentInteraction).ConfigureAwait(false);
          break;
        default:
          break;
      }
    }
    private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
      throw new NotImplementedException();
    }
    private async Task HandleSlash(SocketSlashCommand command)
    {
      throw new NotImplementedException();
    }
    private async Task HandleComponent(SocketMessageComponent component)
    {
      string[] data = component.Data.CustomId.Split(":");
      if (component.User.Id != 402246856752627713)
        return;
      if ((data[0].StartsWith("EditCharacter") || data[0].StartsWith("DeleteCharacter")) && component.User.Id != ulong.Parse(data[1]))
        return;
      switch (data[0])
      {
        case "EditCharacterBack":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Edit", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
            builder.WithButton("Delete", $"DeleteCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);
          break;
        case "EditCharacter":
          await component.UpdateAsync((MessageProperties properties) =>
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
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Prefix, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterName":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Name, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterGender":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Success, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Gender, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterSex":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Sex, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterSpecies":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Species, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterAge":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Age, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterHeight":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Height, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterWeight":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Weight, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterOrientation":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Orientation, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterDescription":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.Description, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterAvatar":
          await component.UpdateAsync((MessageProperties properties) =>
          {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Secondary, null, null, true);
            builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
            properties.Components = builder.Build();
          }).ConfigureAwait(false);

          HandleCharacterEdits(component, CharacterDataType.AvatarURL, data).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

          break;
        case "EditCharacterReference":
          await component.UpdateAsync((MessageProperties properties) =>
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
    public async Task HandleCharacterEdits(SocketMessageComponent component, CharacterDataType type, string[] data)
    {
      switch (type)
      {
        case CharacterDataType.Prefix:
          await component.Message.Channel.SendMessageAsync("Please enter a new prefix.").ConfigureAwait(false);

          var prefixResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (prefixResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Prefix set to {prefixResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Prefix, prefixResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Prefix", $"EditCharacterPrefix:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Name:
          await component.Message.Channel.SendMessageAsync("Please enter a new name.").ConfigureAwait(false);

          var nameResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (nameResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Name set to {nameResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Name, nameResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Name", $"EditCharacterName:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Gender:
          await component.Message.Channel.SendMessageAsync("Please enter a new gender.").ConfigureAwait(false);

          var genderResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (genderResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Gender set to {genderResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Gender, genderResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Gender", $"EditCharacterGender:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Sex:
          await component.Message.Channel.SendMessageAsync("Please enter a new sex.").ConfigureAwait(false);

          var sexResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (sexResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Sex set to {sexResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Sex, sexResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Sex", $"EditCharacterSex:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Species:
          await component.Message.Channel.SendMessageAsync("Please enter a new species.").ConfigureAwait(false);

          var speciesResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (speciesResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Species set to {speciesResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Species, speciesResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Species", $"EditCharacterSpecies:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Age:
          await component.Message.Channel.SendMessageAsync("Please enter a new age.").ConfigureAwait(false);

          var ageResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (ageResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Age set to {ageResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Age, ageResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Age", $"EditCharacterAge:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Height:
          await component.Message.Channel.SendMessageAsync("Please enter a new height.").ConfigureAwait(false);

          var heightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (heightResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Height set to {heightResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Height, heightResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Height", $"EditCharacterHeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Weight:
          await component.Message.Channel.SendMessageAsync("Please enter a new weight.").ConfigureAwait(false);

          var weightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (weightResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Weight set to {weightResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Weight, weightResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Weight", $"EditCharacterWeight:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Orientation:
          await component.Message.Channel.SendMessageAsync("Please enter a new orientation.").ConfigureAwait(false);

          var orientationResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (orientationResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Orientation set to {orientationResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Orientation, orientationResult.Value.Content).ConfigureAwait(false);
            await component.ModifyOriginalResponseAsync(async (MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Orientation", $"EditCharacterOrientation:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();

              Character character = await characters.ViewCharacterByID(ulong.Parse(data[1]), $"{data[1]}:{data[2]}").ConfigureAwait(false);

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
              eBuilder.WithImageUrl(character.ReferenceURL);
              eBuilder.WithCurrentTimestamp();
              eBuilder.WithColor(new Color(0xcc70ff));
              eBuilder.WithFooter("Made by SnowyStarfall (Snowy#0364)", component.User.GetAvatarUrl(ImageFormat.Gif));
              properties.Embeds = new Embed[1] { eBuilder.Build() };
            }).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.Description:
          await component.Message.Channel.SendMessageAsync("Please enter a new description.").ConfigureAwait(false);

          var descriptionResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (descriptionResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Description set to {descriptionResult.Value.Content}").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.Description, descriptionResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Description", $"EditCharacterDescription:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.AvatarURL:
          await component.Message.Channel.SendMessageAsync("Please send a new avatar.").ConfigureAwait(false);

          var avatarResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Attachments.First() != null), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (avatarResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Avatar set.").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.AvatarURL, avatarResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Avatar", $"EditCharacterAvatar:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);
          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
        case CharacterDataType.ReferenceURL:
          await component.Message.Channel.SendMessageAsync("Please send a new reference.").ConfigureAwait(false);

          var referenceResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == ulong.Parse(data[1])) && (x.Channel.Id == component.Message.Channel.Id) && (x.Attachments.First() != null), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

          if (referenceResult.IsSuccess)
          {
            await component.Message.Channel.SendMessageAsync($"Reference set.").ConfigureAwait(false);
            await characters.EditCharacter(ulong.Parse(data[1]), $"{data[1]}:{data[2]}", CharacterDataType.ReferenceURL, referenceResult.Value.Content).ConfigureAwait(false);
            await component.UpdateAsync((MessageProperties properties) =>
            {
              ComponentBuilder builder = new ComponentBuilder();
              builder.WithButton("Reference", $"EditCharacterReference:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Primary);
              builder.WithButton("Back", $"EditCharacter:{data[1]}:{data[2]}:{data[3]}", ButtonStyle.Danger);
              properties.Components = builder.Build();
            }).ConfigureAwait(false);
            await component.Message.ModifyAsync(async (MessageProperties properties) => properties.Embed = await EmbedHandler.CreateCharacterEmbedAsync(ulong.Parse(data[1]), data[2], new string[] { component.User.Username, component.User.Discriminator, component.User.GetAvatarUrl() }).ConfigureAwait(false)).ConfigureAwait(false);

          }
          else
            await component.Message.Channel.SendMessageAsync("TImed out.").ConfigureAwait(false);
          break;
      }
    }
  }
}
