using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SnowyBot.Services
{
  public class FunService : ModuleBase<SocketCommandContext>
  {
    public Random random = new Random();
    [Command("Question")]
    public async Task Question()
    {
      int choice = random.Next(0, 11);
      await ReplyAsync(choice == 0 ? "yeh" : "noh").ConfigureAwait(false);
    }
  }
}
