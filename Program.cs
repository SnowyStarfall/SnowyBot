using System.Threading.Tasks;
using System.Timers;
using SnowyBot.Services;

namespace SnowyBot
{
  public static class Program
  {
    private static Task Main() => DiscordService.InitializeAsync();
  }
}
