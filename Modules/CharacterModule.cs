using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Handlers;
using SnowyBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
  public class CharacterModule : ModuleBase
  {
    public Random random = new Random();
    public readonly Characters characters;
    public CharacterModule(Characters _characters) => characters = _characters;

    [Command("cha")]
    [Alias(new string[] { "character add", "character create", "char add", "char create" })]
    public async Task AddCharacter()
    {
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

      await Context.Channel.SendMessageAsync("This feature is a work in progress. Please report any bugs to <@402246856752627713>.\n" +
                                             "Please be sure to enter responses within **5 minutes** or your progress will be lost.\n" +
                                             "**These responses can be edited later.** If you can't finish a response within **5 minutes**, don't be afraid to enter a placeholder.").ConfigureAwait(false);

      await Context.Channel.SendMessageAsync("Please enter a prefix.").ConfigureAwait(false);

      var prefixResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (prefixResult.IsSuccess)
      {
        prefix = prefixResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a name.").ConfigureAwait(false);

      var nameResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (nameResult.IsSuccess)
      {
        name = nameResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
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
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a sex.").ConfigureAwait(false);

      var sexResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (sexResult.IsSuccess)
      {
        sex = sexResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a species.").ConfigureAwait(false);

      var speciesResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (speciesResult.IsSuccess)
      {
        species = speciesResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter an age.").ConfigureAwait(false);

      var ageResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (ageResult.IsSuccess)
      {
        age = ageResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a height.").ConfigureAwait(false);

      var heightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (heightResult.IsSuccess)
      {
        height = heightResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a weight.").ConfigureAwait(false);

      var weightResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (weightResult.IsSuccess)
      {
        weight = weightResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter an orientation.").ConfigureAwait(false);

      var orientationResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (orientationResult.IsSuccess)
      {
        orientation = orientationResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please enter a description. This can be edited later.").ConfigureAwait(false);

      var descriptionResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (descriptionResult.IsSuccess)
      {
        description = descriptionResult.Value.Content;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please send an avatar picture, or enter \"Skip\" to skip.").ConfigureAwait(false);

      var avatarResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Attachments.Count > 0), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (avatarResult.IsSuccess)
      {
        if (string.Equals(avatarResult.Value.Content, "skip", StringComparison.OrdinalIgnoreCase))
          avatarURL = "";
        else
          avatarURL = avatarResult.Value.Attachments.First().Url;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Please send a reference picture, or enter \"Skip\" to skip.").ConfigureAwait(false);

      var referenceResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Attachments.Count > 0 || x.Content.ToLower() == "skip"), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);

      if (referenceResult.IsSuccess)
      {
        if (string.Equals(referenceResult.Value.Content, "skip", StringComparison.OrdinalIgnoreCase))
          referenceURL = "";
        else
          referenceURL = referenceResult.Value.Attachments.First().Url;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Character added!").ConfigureAwait(false);

      await characters.AddCharacter(Context.User.Id, DateTime.Now, prefix, name, gender, sex, species, age, height, weight, orientation, description, avatarURL, referenceURL).ConfigureAwait(false);
    }
    [Command("chv")]
    [Alias(new string[] { "character view", "char view" })]
    public async Task ViewCharacter([Remainder] string name)
    {
      Character character = await characters.ViewCharacter(Context.User.Id, name).ConfigureAwait(false);

      if (character == null)
      {
        await Context.Channel.SendMessageAsync("Character not found. Are you sure you spelled the name correctly?").ConfigureAwait(false);
        return;
      }

      EmbedBuilder builder = new EmbedBuilder();
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
      builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713ul).ConfigureAwait(false)).GetAvatarUrl(ImageFormat.Gif) ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

      string[] id = character.CharacterID.Split(":");

      ComponentBuilder cBuilder = new ComponentBuilder();
      cBuilder.WithButton("Edit", $"EditCharacter:{Context.User.Id}:{id[1]}:{Context.Channel.Id}", ButtonStyle.Primary);
      cBuilder.WithButton("Delete", $"DeleteCharacter:{Context.User.Id}:{id[1]}:{Context.Channel.Id}", ButtonStyle.Danger);

      await Context.Channel.SendMessageAsync(null, false, builder.Build(), null, null, null, cBuilder.Build()).ConfigureAwait(false);
    }
    [Command("chd")]
    [Alias(new string[] { "character delete", "char delete" })]
    public async Task DeleteCharacter([Remainder] string name)
    {
      string[] result = name.Split(" ");

      Character character = await characters.ViewCharacter(Context.User.Id, result[0]).ConfigureAwait(false);

      if (character == null)
      {
        await Context.Channel.SendMessageAsync("Character not found. Are you sure you spelled the name correctly?").ConfigureAwait(false);
        return;
      }

      EmbedBuilder builder = new EmbedBuilder();
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
      builder.WithFooter("Made by SnowyStarfall (Snowy#0364)", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl());

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
  }
}
