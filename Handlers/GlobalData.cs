using Discord;
using Newtonsoft.Json;
using SnowyBot.Structs;
using SnowyBot.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
  public class GlobalData
  {
    public static string ConfigPath { get; set; } = "config.json";
    public static BotConfig Config { get; set; }

    public async Task InitializeAsync()
    {
      string json;

      if (!File.Exists(ConfigPath))
      {
        json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
        File.WriteAllText("config.json", json, new UTF8Encoding(false));
        await LoggingService.LogAsync("Bot", LogSeverity.Error, "No Config file found. A new one has been generated. Please close the & fill in the required section.").ConfigureAwait(false);
        await Task.Delay(-1).ConfigureAwait(false);
      }

      json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
      Config = JsonConvert.DeserializeObject<BotConfig>(json);
    }

    private static BotConfig GenerateNewConfig() => new BotConfig
    {
      DiscordToken = "ODE0NzgwNjY1MDE4MzE4ODc4.YDi1oA.eqQAMiBjrleG2IbEzFalQlFE5KY",
      DefaultPrefix = "!",
      GameStatus = $"music for {DiscordService.lavaNode.Players.Count()} servers.",
      BlacklistedChannels = new List<ulong>()
    };
  }
}
