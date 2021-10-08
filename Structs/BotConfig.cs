﻿using Discord;
using System.Collections.Generic;

namespace SnowyBot.Structs
{
  public class BotConfig
  {
    public string DiscordToken { get; set; }
    public string LavaAuthorization { get; set; }
    public string DefaultPrefix { get; set; }
    public string GameStatus { get; set; }
    public List<ulong> BlacklistedChannels { get; set; }
  }
}
