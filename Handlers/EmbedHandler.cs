using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;

namespace SnowyBot.Handlers
{
  public static class EmbedHandler
  {
    /* This file is where we can store all the Embed Helper Tasks (So to speak). 
         We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
         All the Tasks here are also static which means we can call them from anywhere in our program. */
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
