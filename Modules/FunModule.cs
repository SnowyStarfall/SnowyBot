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
  public class FunModule : ModuleBase
  {
    public readonly Guilds guilds;
    public FunModule(Guilds _guilds) => guilds = _guilds;

    public Random random = new Random();

    [Command("Question")]
    [Alias(new string [] {"Q"})]
    public async Task QuestionAsync([Remainder] string question = null)
    {
      Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
      int choice = random.Next(0, 101);
      if (Context.Message.Content.Contains("love me", StringComparison.OrdinalIgnoreCase) && !Context.Message.Content.Contains("not love me", StringComparison.OrdinalIgnoreCase))
        await Context.Message.ReplyAsync("yeh").ConfigureAwait(false);
      else
        await Context.Message.ReplyAsync(choice == 0 ? "NOH" : choice >= 1 && choice <= 11 ? "yeh" : choice >= 12 && choice <= 99 ? "noh" : "why are you gae").ConfigureAwait(false);
    }
    [Command("Snort")]
    public async Task Snort()
    {
      Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
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
    [Command("8ball")]
    public async Task EightBall([Remainder] string question)
    {
      Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");
      int type = random.Next(0, 2);
      int response = type == 0 ? random.Next(0, 11) : type == 2 ? random.Next(11, 16) : random.Next(16, 21);
      string[] responses = new string[] { "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
                                          "You may rely on it.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
                                          "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
                                          "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."};
      await Context.Message.ReplyAsync($"{responses[response]}").ConfigureAwait(false);
    }
    [Command("a")]
    public async Task A([Remainder] string letter)
    {
      Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");

      int caps = random.Next(0, 2);
      int num = random.Next(10, 200);

      string reply = "";
      string alpha;

      if (!(letter[0].ToString().ToLower()[0] >= 'a' && letter[0].ToString().ToLower()[0] <= 'z') && !(letter[0].ToString().ToLower()[0] >= 'A' && letter[0].ToString().ToLower()[0] <= 'Z'))
        alpha = "A";
      else
        alpha = caps == 0 ? $"{letter[0].ToString().ToLower()}" : $"{letter[0].ToString().ToUpper()}";

      for (int i = 0; i < num; i++)
      {
        reply += alpha;
      }

      await Context.Message.ReplyAsync(reply).ConfigureAwait(false);
    }
    [Command("Info")]
    public async Task Info()
    {
      Console.WriteLine($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator} : {Context.Message.Author.Id} in {Context.Guild.Name} : {Context.Guild.Id}");

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
