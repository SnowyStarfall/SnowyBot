using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using SnowyBot.Handlers;
using System.Linq;
using System.Collections.Generic;

namespace SnowyBot.Services
{
  public sealed class BotService
  {
    public LavaLinkAudio Audio { get; set; }
    public async Task<Embed> DisplayInfoAsync(SocketCommandContext context)
    {
      var fields = new List<EmbedFieldBuilder>
      {
        new EmbedFieldBuilder
        {
          Name = "Client Info",
          Value = $"Current Server: {context.Guild.Name} - Prefix: {GlobalData.Config.DefaultPrefix}",
          IsInline = false
        },
        new EmbedFieldBuilder
        {
          Name = "Guild Info",
          Value = $"Current People: {context.Guild.Users.Count(x => !x.IsBot)} - Current Bots: {context.Guild.Users.Count(x => x.IsBot)} - Overall Users: {context.Guild.Users.Count}\n" +
          $"Text Channels: {context.Guild.TextChannels.Count} - Voice Channels: {context.Guild.VoiceChannels.Count}",
          IsInline = false
        }
      };

      var embed = await Task.Run(() => new EmbedBuilder
      {
        Title = "Info",
        ThumbnailUrl = context.Guild.IconUrl,
        Timestamp = DateTime.UtcNow,
        Color = Color.DarkOrange,
        Footer = new EmbedFooterBuilder { Text = "Created by SnowyStarfall, service via Victoria", IconUrl = context.Client.CurrentUser.GetAvatarUrl() },
        Fields = fields
      }).ConfigureAwait(false);

      return embed.Build();
    }

  }
}
