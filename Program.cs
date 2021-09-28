using System.Threading.Tasks;
using SnowyBot.Services;

namespace SnowyBot
{
  public static class Program
  {
    private static Task Main()
        => new DiscordService().InitializeAsync();
  }
}
