using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using SnowyBot.Database;
using SnowyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;
using static SnowyBot.SnowyBotUtils;

namespace SnowyBot.Modules
{
  public class DevModule : ModuleBase
  {
    public Guilds guilds;
    public GuildContext context;
    public DevModule(Guilds _guilds, GuildContext _context)
    {
      guilds = _guilds;
      context = _context;
    }

    [Command("User")]
    [RequireOwner]
    public async Task User([Remainder] string ID)
    {
      IUser user = await DiscordService.client.GetUserAsync(ulong.Parse(ID)).ConfigureAwait(false);
      RestUser rest = user as RestUser;
      EmbedBuilder builder = new();
      builder.WithAuthor(Context.User.Username, Context.User.GetAvatarUrl());
      builder.WithThumbnailUrl(user.GetAvatarUrl());
      builder.WithTitle(user.Username + "#" + user.Discriminator);
      builder.WithColor(new Color(0xcc70ff));
      if (rest != null && rest.Activity != null)
        builder.WithDescription($"Activity:\nName - {rest.Activity.Name}\nType - {rest.Activity.Type}\nDetails - {rest.Activity.Details}");
      builder.AddField("Is Bot:", user.IsBot ? "Yes" : "No", true);
      if(rest != null)
      {
        builder.AddField("Created At:", rest.CreatedAt.ToString(), true);
        builder.AddField("Rest Status:", rest.Status.ToString(), true);
      }
      builder.AddField("Mention:", user.Mention);

      await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
    }
    [Command("Invite")]
    [RequireOwner]
    public async Task Invite()
    {
      await Context.Channel.SendMessageAsync("https://discord.com/api/oauth2/authorize?client_id=814780665018318878&permissions=8&scope=bot").ConfigureAwait(false);
    }
    [Command("Update")]
    [RequireOwner]
    public async Task Update()
    {
      List<ITextChannel> channels = new();
      foreach(Guild guild in context.Guilds)
      {
        SocketGuild g = await Context.Client.GetGuildAsync(guild.ID).ConfigureAwait(false) as SocketGuild;
        if(g != null)
        {
          ITextChannel c = g.GetChannel(guild.ChangelogID) as ITextChannel;
          if(c != null)
            channels.Add(c);
        }
      }
      EmbedBuilder builder = new();
      builder.WithTitle($"{SnowyLeftLine}{SnowyLine}{SnowyRightLine} Changelog v1.0! {SnowyLeftLine}{SnowyLine}{SnowyRightLine}");
      builder.WithColor(new Color(0xcc70ff));
      builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
      builder.WithFooter($"Bot made by SnowyStarfall - Snowy#8364", DiscordService.Snowy.GetAvatarUrl(ImageFormat.Png));
      builder.WithDescription($"{SnowyUniversalStrong} {SnowySmallButton} **Added** {SnowySmallButton} {SnowyUniversalStrong}\n" +
                              $"{SnowySmallButton} Welcome message config option. `!welcome <text>`\n" +
                              $"{SnowySmallButton} Goodbye message config option. `!goodbye <text>`\n" +
                              $"{SnowySmallButton} Delete Music Posts config option. `!deletemusic`\n" +
                              $"{SnowySmallButton} Bot Updates config option. `changelog <channel>`\n" +
                              $"{SnowySmallButton} Search command for music. `!search <query>`\n" +
                              $"{SnowySmallButton} Added command post correction support.\n" +
                              $"{SnowySmallButton} QRemove command now supports ranges. `!qr <index1> <index2>`\n" +
                              $"{SnowySmallButton} Clear command for music (different from stop as it preserves the current playing song). `!qclear`\n" +
                              $"{SnowySmallButton} Reactive Roles. `!roles`\n" +
                              $"{SnowySmallButton} Bot now leaves after five minutes of inactivity.\n" +
                              $"{SnowySmallButton} WebHooks can now be deleted. React with :x: within ten minutes to delete them.\n" +
                              $"{SnowySmallButton} You can now input the amount of times to loop a track through the!loop command. `!loop <amount>`\n" +
                              $"{SnowySmallButton} You can now search for the lyrics or art of current music track via `!lyrics` or `!art`.\n\n" +
                              $"{SnowyUniversalStrong} {SnowySmallButton} **Fixed** {SnowySmallButton} {SnowyUniversalStrong}\n" +
                              $"{SnowySmallButton} Queue should play correctly now.\n" +
                              $"{SnowySmallButton} Overhauled message formatting for music.\n" +
                              $"{SnowySmallButton} Music commands should now only run when appropriate, i.e.you are in a VC when running them.\n" +
                              $"{SnowySmallButton} Increased character creation timeout time from 2 minutes to 5 minutes.\n" +
                              $"{SnowySmallButton} Fixed improper grammar in character creation.\n" +
                              $"{SnowySmallButton} Fixed skipping requiring double input.\n" +
                              $"{SnowySmallButton} Bot now allows negative input for jump command.\n" +
                              $"{SnowySmallButton} Status will now update with active players.\n" +
                              $"{SnowySmallButton} Soundcloud searches work now.\n" +
                              $"{SnowySmallButton} Link should always play correct video.");
      await guilds.SendChangelogUpdate(channels, builder.Build()).ConfigureAwait(false);
    }
    [Command("Image")]
    [RequireOwner]
    public async Task Image([Remainder] string query)
    {
      IEnumerable<ImgurResult> results = await SearchAsync(query).ConfigureAwait(false);
      if(results == null)
      {
        await Context.Channel.SendMessageAsync("No results found.").ConfigureAwait(false);
        return;
      }
      await Context.Channel.SendMessageAsync(results.ToList().First().Url).ConfigureAwait(false);
    }
  }
}
