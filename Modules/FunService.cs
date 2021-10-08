using Discord;
using Discord.Commands;
using SnowyBot.Database;
using SnowyBot.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Modules
{
  public class FunService : ModuleBase
  {
    public readonly Guilds guilds;
    public FunService(Guilds _guilds) => guilds = _guilds;

    public Random random = new Random();

    [Command("Question")]
    public async Task QuestionAsync([Remainder] string question)
    {
      int choice = random.Next(0, 11);
      if (Context.Message.Content.Contains("love me", StringComparison.OrdinalIgnoreCase) && !Context.Message.Content.Contains("not love me", StringComparison.OrdinalIgnoreCase))
        await Context.Message.ReplyAsync("yeh").ConfigureAwait(false);
      else
        await Context.Message.ReplyAsync(choice == 0 ? "yeh" : "noh").ConfigureAwait(false);
    }
    [Command("Snort")]
    public async Task Snort()
    {
      int markdown = random.Next(0, 7);
      int size = random.Next(0, 6);

      string markdownStr = markdown == 0 ? "*" :
                           markdown == 1 ? "**" :
                           markdown == 2 ? "***" :
                           markdown == 3 ? "_" :
                           markdown == 4 ? "*_" :
                           markdown == 5 ? "**_" :
                           "***_";

      string sizeStr = size == 0 ? "snort" :
                       size == 1 ? "SNORT" :
                       size == 2 ? "sɴᴏʀᴛ" :
                       size == 3 ? "ˢⁿᵒʳᵗ" :
                       size == 4 ? "ₛₙₒᵣₜ" :
                       "ˢᴺᴼᴿᵀ";

      await Context.Channel.SendMessageAsync($"-{markdownStr}{sizeStr}{new string(markdownStr.ToCharArray().Reverse().ToArray())}-").ConfigureAwait(false);
      await Context.Message.DeleteAsync().ConfigureAwait(false);
    }
    [Command("Info")]
    public async Task Info()
    {
      SocketCommandContext context = Context as SocketCommandContext;
      int numUsers = 0;
      int numBots = 0;
      foreach (IGuildUser user in context.Guild.Users)
      {
        if (user.IsBot)
          numBots++;
        else
          numUsers++;
      }
      EmbedBuilder builder = new EmbedBuilder();
      builder.WithTitle($"{context.Guild.Name}");
      builder.WithThumbnailUrl(context.Guild.IconUrl);
      builder.WithDescription($"{context.Guild.Description}");
      builder.WithColor(new Color(0x53f2a0));
      builder.AddField("Prefix", $"{guilds.GetGuildPrefix(context.Guild.Id).Result ?? "!"}", false);
      builder.AddField("Owner", context.Guild.Owner.Username, true);
      builder.AddField("Creation Date", $"{context.Guild.CreatedAt}");
      builder.AddField("Boost Level", $"{context.Guild.PremiumTier.ToString().Insert(4, " ")}", true);
      builder.AddField("Text Channels", $"{context.Guild.TextChannels.Count}", true);
      builder.AddField("Voice Channels", $"{context.Guild.VoiceChannels.Count}", true);
      builder.AddField("Users", $"{numUsers}", true);
      builder.AddField("Bots", $"{numBots}", true);
      builder.AddField("Total", $"{context.Guild.MemberCount}", true);
      builder.WithTimestamp(DateTime.UtcNow);
      builder.WithFooter("Created by SnowyStarfall - Snowy#0364", context.Client.CurrentUser.GetAvatarUrl());
      await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
    }
  }
}
