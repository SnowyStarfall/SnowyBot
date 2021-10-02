using Discord;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
  public static class EmbedHandler
  {
    /* This file is where we can store all the Embed Helper Tasks (So to speak). 
         We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
         All the Tasks here are also static which means we can call them from anywhere in our program. */
    public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
    {
      return await Task.Run(() => (new EmbedBuilder()
          .WithTitle(title)
          .WithDescription(description)
          .WithColor(color)
          .WithCurrentTimestamp().Build())).ConfigureAwait(false);
    }

    public static async Task<Embed> CreateErrorEmbed(string source, string error)
    {
      return await Task.Run(() => new EmbedBuilder()
          .WithTitle($"Error occurred from - {source}")
          .WithDescription($"**Error Details**: \n{error}")
          .WithColor(Color.DarkRed)
          .WithCurrentTimestamp().Build()).ConfigureAwait(false);
    }
  }
}
