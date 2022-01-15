using Discord.Commands;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
  public class SearchModule : ModuleBase
  {

    [Command("Imgur")]
    [Alias(new[] { "Meme", "Img" })]
    public async Task Imgur([Remainder] string query)
    {

    }
  }
}
