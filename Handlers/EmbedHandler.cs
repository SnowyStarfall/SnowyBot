using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;

namespace SnowyBot.Handlers
{
  public class EmbedHandler
  {
    public static Characters characters;
    public EmbedHandler(Characters _characters) => characters = _characters;

    public static Embed CreateBasicEmbed(string title, string description, Color color)
    {
      EmbedBuilder builder = new EmbedBuilder();
      builder.WithThumbnailUrl("https://i.vgy.me/TdgsND.png");
      builder.WithTitle(title);
      builder.WithDescription(description);
      builder.WithColor(color);
      builder.WithCurrentTimestamp();
      return builder.Build();
    }

    public static Embed CreateErrorEmbed(string source, string error)
    {
      EmbedBuilder builder = new EmbedBuilder();
      builder.WithThumbnailUrl("https://i.vgy.me/6msNWf.png");
      builder.WithTitle($"Error occurred from - {source}");
      builder.WithDescription($"**Error Details**: \n{error}");
      builder.WithColor(Color.DarkRed);
      builder.WithCurrentTimestamp();
      return builder.Build();
    }

    public static Embed CreateMusicListEmbed(IReadOnlyCollection<LavaTrack> tracks)
    {
      EmbedBuilder builder = new EmbedBuilder();

      builder.WithThumbnailUrl("https://i.vgy.me/TdgsND.png");
      builder.WithTitle("Music");
      builder.WithDescription("React with the number of the result that you want.");
      builder.WithColor(0xcc70ff);
      builder.WithCurrentTimestamp();

      int trackNum = 1;
      if (trackNum <= 10)
      {
        foreach (LavaTrack lavaTrack in tracks)
        {
          builder.AddField($":{NumToEmoji(trackNum)}: - {lavaTrack.Title} : {lavaTrack.Author}", lavaTrack.Url, false);
          trackNum++;
        }
      }

      return builder.Build();
    }

    public static async Task<Embed> CreateCharacterEmbedAsync(ulong userID, string characterID, string[] userData)
    {
      Character character = await characters.ViewCharacterByID(userID, $"{userID}:{characterID}").ConfigureAwait(false);

      EmbedBuilder builder = new EmbedBuilder();
      builder.WithAuthor($"{userData[0]}#{userData[1]}", userData[2]);
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
      builder.WithImageUrl(character.ReferenceURL);
      builder.WithCurrentTimestamp();
      builder.WithColor(new Color(0xcc70ff));
      builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
      return builder.Build();
    }

    public static string NumToEmoji(int num)
    {
      return num == 0 ? "0️⃣" :
             num == 1 ? "1️⃣" :
             num == 2 ? "2️⃣" :
             num == 3 ? "3️⃣" :
             num == 4 ? "4️⃣" :
             num == 5 ? "5️⃣" :
             num == 6 ? "6️⃣" :
             num == 7 ? "7️⃣" :
             num == 8 ? "8️⃣" :
             num == 9 ? "9️⃣" :
             num == 10 ? "🔟" :
             "Unknown";
    }
  }
}
