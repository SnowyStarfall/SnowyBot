using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using SnowyBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;

namespace SnowyBot.Modules
{
  public class DevModule : ModuleBase
  {
    [Command("User")]
    [RequireOwner]
    public async Task User([Remainder] string ID)
    {
      IUser user = await DiscordService.client.GetUserAsync(ulong.Parse(ID)).ConfigureAwait(false);
      RestUser rest = user as RestUser;
      SocketUser socket = user as SocketUser;
      EmbedBuilder builder = new EmbedBuilder();
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
  }
}
