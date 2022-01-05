using System;
using System.Threading.Tasks;
using SnowyBot.Services;
using Discord.Commands;
using static Discord.MentionUtils;
using Discord;
using Discord.WebSocket;
using SnowyBot.Database;

namespace SnowyBot.Modules
{
  public class RoleModule : ModuleBase
  {
    Guilds guilds;
    EmbedBuilder builder;
    public RoleModule(Guilds _guilds)
    {
      guilds = _guilds;
    }

    [Command("roles")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task Role()
    {
      builder = new EmbedBuilder();
      builder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
      builder.WithTitle("Reactive Roles");
      builder.WithDescription("Pick an option:");
      builder.AddField("1. New Message", "Add a new reactive message.");
      builder.AddField("2. Edit Message", "Edit text for a reactive message.");
      builder.AddField("3. Edit Roles", "Edit roles for a reactive message.");
      builder.WithCurrentTimestamp();
      builder.WithColor(new Color(0xcc70ff));
      builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713ul).ConfigureAwait(false)).GetAvatarUrl(ImageFormat.Gif) ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

      IUserMessage embed = await Context.Channel.SendMessageAsync(null, false, builder.Build());

      var option = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
      if (!option.IsSuccess || !int.TryParse(option.Value.Content, out _) || int.Parse(option.Value.Content) < 1 || int.Parse(option.Value.Content) > 3)
      {
        await Context.Channel.SendMessageAsync("Timed out or incorrect response.");
        return;
      }
      int num = int.Parse(option.Value.Content);

      Guild guild = await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false);
      SocketTextChannel channel;
      IUserMessage message;

      switch (num)
      {
        case 1:
          await option.Value.DeleteAsync().ConfigureAwait(false);
          await SetupRoles().ConfigureAwait(false);
          break;
        case 2:
          await option.Value.DeleteAsync().ConfigureAwait(false);

          if ((await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false)).Roles == string.Empty || (await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false)).Roles == null)
          {
            IUserMessage m0 = await Context.Channel.SendMessageAsync("Please first create a message before adding roles.");
            await Task.Delay(5000).ConfigureAwait(false);
            await m0.DeleteAsync().ConfigureAwait(false);
            return;
          }

          IUserMessage m1 = await Context.Channel.SendMessageAsync("Please mention the channel which the message resides in.").ConfigureAwait(false);
          var c1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
          if (c1.IsSuccess)
          {
            if (!TryParseChannel(c1.Value.Content, out ulong channelID1))
            {
              IUserMessage m2 = await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
              await m1.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m2.DeleteAsync().ConfigureAwait(false);
              return;
            }
            channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as SocketTextChannel;
            if (channel == null)
            {
              IUserMessage m3 = await Context.Channel.SendMessageAsync("Channel does not exist or I lack permissions to see it.").ConfigureAwait(false);
              await m1.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m3.DeleteAsync().ConfigureAwait(false);
              return;
            }
          }
          else
          {
            IUserMessage m4 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
            await m1.DeleteAsync().ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await m4.DeleteAsync().ConfigureAwait(false);
            return;
          }

          IUserMessage m5 = await Context.Channel.SendMessageAsync("Please enter the ID of the message.").ConfigureAwait(false);
          var mr1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
          if (mr1.IsSuccess)
          {
            if (!ulong.TryParse(mr1.Value.Content, out ulong nessageIDParsed))
            {
              IUserMessage m6 = await Context.Channel.SendMessageAsync("Please enter a valid message ID.").ConfigureAwait(false);
              await m5.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m6.DeleteAsync().ConfigureAwait(false);
              return;
            }
            if (await channel.GetMessageAsync(nessageIDParsed).ConfigureAwait(false) == null)
            {
              IUserMessage m7 = await Context.Channel.SendMessageAsync("This message does not exist.").ConfigureAwait(false);
              await m5.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m7.DeleteAsync().ConfigureAwait(false);
              return;
            }
            else
            {
              message = await channel.GetMessageAsync(nessageIDParsed).ConfigureAwait(false) as IUserMessage;
            }
          }
          else
          {
            IUserMessage m8 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
            await m5.DeleteAsync().ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await m8.DeleteAsync().ConfigureAwait(false);
            return;
          }

          string text = string.Empty;
          IUserMessage m9 = await Context.Channel.SendMessageAsync("Please enter the text to update to.").ConfigureAwait(false);
          var mr2 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
          if (mr2.IsSuccess)
          {
            text = mr2.Value.Content;
          }
          else
          {
            IUserMessage m10 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
            await m9.DeleteAsync().ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await m10.DeleteAsync().ConfigureAwait(false);
            return;
          }

          await message.ModifyAsync((MessageProperties properties) =>
          {
            properties.Content = text;
          }).ConfigureAwait(false);
          IUserMessage m19 =  await Context.Channel.SendMessageAsync($"Updated: {message.GetJumpUrl()}");
          await Task.Delay(5000).ConfigureAwait(false);
          await m19.DeleteAsync().ConfigureAwait(false);
          break;
        case 3:
          await option.Value.DeleteAsync().ConfigureAwait(false);

          if ((await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false)).Roles == string.Empty || (await guilds.GetGuild(Context.Guild.Id).ConfigureAwait(false)).Roles == null)
          {
            IUserMessage m0 = await Context.Channel.SendMessageAsync("Please first create a message before adding roles.");
            await Task.Delay(5000).ConfigureAwait(false);
            await m0.DeleteAsync().ConfigureAwait(false);
            return;
          }

          IUserMessage m11 = await Context.Channel.SendMessageAsync("Please mention the channel which the message resides in.").ConfigureAwait(false);
          var c2 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
          if (c2.IsSuccess)
          {
            if (!TryParseChannel(c2.Value.Content, out ulong channelID2))
            {
              IUserMessage m12 = await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
              await m11.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m12.DeleteAsync().ConfigureAwait(false);
              return;
            }
            channel = await Context.Guild.GetChannelAsync(channelID2).ConfigureAwait(false) as SocketTextChannel;
            if (channel == null)
            {
              IUserMessage m13 = await Context.Channel.SendMessageAsync("Channel does not exist or I lack permissions to see it.").ConfigureAwait(false);
              await m11.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m13.DeleteAsync().ConfigureAwait(false);
              return;
            }
          }
          else
          {
            IUserMessage m14 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
            await m11.DeleteAsync().ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await m14.DeleteAsync().ConfigureAwait(false);
            return;
          }

          IUserMessage m15 = await Context.Channel.SendMessageAsync("Please enter the ID of the message.").ConfigureAwait(false);
          var mr3 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
          if (mr3.IsSuccess)
          {
            if (!ulong.TryParse(mr3.Value.Content, out ulong nessageIDParsed1))
            {
              IUserMessage m16 = await Context.Channel.SendMessageAsync("Please enter a valid message ID.").ConfigureAwait(false);
              await m15.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m16.DeleteAsync().ConfigureAwait(false);
              return;
            }
            if (await channel.GetMessageAsync(nessageIDParsed1).ConfigureAwait(false) == null)
            {
              IUserMessage m17 = await Context.Channel.SendMessageAsync("This message does not exist.").ConfigureAwait(false);
              await m15.DeleteAsync().ConfigureAwait(false);
              await Task.Delay(5000).ConfigureAwait(false);
              await m17.DeleteAsync().ConfigureAwait(false);
              return;
            }
            else
            {
              message = await channel.GetMessageAsync(nessageIDParsed1).ConfigureAwait(false) as IUserMessage;
            }
          }
          else
          {
            IUserMessage m18 = await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
            await m15.DeleteAsync().ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            await m18.DeleteAsync().ConfigureAwait(false);
            return;
          }

          builder = new EmbedBuilder();
          builder.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
          builder.WithTitle("Reactive Roles");
          builder.WithDescription($"Channel: {channel.Mention}\nMessage: {message.GetJumpUrl()}");
          builder.AddField("Add Role", "Add a new role to this reactive message.");
          builder.AddField("Remove Role", "Remove a role from this reactive message.");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713ul).ConfigureAwait(false)).GetAvatarUrl(ImageFormat.Gif) ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

          ComponentBuilder componentBuilder = new();
          componentBuilder.WithButton("Add Role", $"ReactiveRolesAdd:{Context.User.Id}:{channel.Guild.Id}:{channel.Id}:{message.Id}", ButtonStyle.Primary);
          componentBuilder.WithButton("Remove Role", $"ReactiveRolesRemove:{Context.User.Id}:{channel.Guild.Id}:{channel.Id}:{message.Id}", ButtonStyle.Primary);
          componentBuilder.WithButton("Back", $"ReactiveRolesBack:{Context.User.Id}:{channel.Guild.Id}:{channel.Id}:{message.Id}", ButtonStyle.Danger);

          await embed.ModifyAsync((MessageProperties properties) =>
          {
            properties.Embed = builder.Build();
          });
          await Context.Channel.SendMessageAsync(null, false, builder.Build(), null, null, null, componentBuilder.Build()).ConfigureAwait(false);
          break;
      }
    }
    public async Task SetupRoles()
    {
      bool makePost = true;
      await Context.Channel.SendMessageAsync("Would you like to...\n`1`. Provide a preexisting message ID.\n`2`. Let me handle the post.").ConfigureAwait(false);
      var mpr = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
      if (mpr.IsSuccess)
      {
        if (mpr.Value.Content == "1")
          makePost = false;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please enter 1 or 2.").ConfigureAwait(false);
        return;
      }

      IMessageChannel channel = null;
      await Context.Channel.SendMessageAsync($"Please mention the channel {(makePost ? "I should post in." : "the message is in.")}").ConfigureAwait(false);
      var cr1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
      if (cr1.IsSuccess)
      {
        if (!TryParseChannel(cr1.Value.Content, out ulong channelID1))
        {
          await Context.Channel.SendMessageAsync("Please enter a valid channel mention.").ConfigureAwait(false);
          return;
        }
        channel = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
        return;
      }

      string emoji = "";
      await Context.Channel.SendMessageAsync("Please mention the emoji to use for this role. (Do not use emojis from a server that I'm not in.)").ConfigureAwait(false);
      var e1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
      if (e1.IsSuccess)
      {
        bool flag1 = Emoji.TryParse(e1.Value.Content, out Emoji emojiID1);
        bool flag2 = Emote.TryParse(e1.Value.Content, out Emote emoteID1);
        if (!flag1 && !flag2)
        {
          await Context.Channel.SendMessageAsync("Please enter a valid emoji.").ConfigureAwait(false);
          return;
        }
        if (flag1)
          emoji = emojiID1.Name;
        if (flag2)
          emoji = emoteID1.ToString();
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      ulong role = 0;
      await Context.Channel.SendMessageAsync("Please mention the role to assign to the emoji.").ConfigureAwait(false);
      var r1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
      if (r1.IsSuccess)
      {
        if (!TryParseRole(r1.Value.Content, out ulong roleID))
        {
          await Context.Channel.SendMessageAsync("Please enter a valid role mention.").ConfigureAwait(false);
          return;
        }
        role = roleID;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed out.").ConfigureAwait(false);
        return;
      }

      bool flag3 = Emoji.TryParse(e1.Value.Content, out Emoji emojiID);
      bool flag4 = Emote.TryParse(e1.Value.Content, out Emote emoteID);

      if (!makePost)
      {
        await Context.Channel.SendMessageAsync("Please provide the ID of the message.").ConfigureAwait(false);

        IMessage m1 = null;

        var mr1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

        if (mr1.IsSuccess)
        {
          if (!ulong.TryParse(cr1.Value.Content, out ulong messageIDParsed1))
          {
            await Context.Channel.SendMessageAsync("Please enter a valid message ID.").ConfigureAwait(false);
            return;
          }
          if (await channel.GetMessageAsync(messageIDParsed1).ConfigureAwait(false) == null)
          {
            await Context.Channel.SendMessageAsync("Please enter a valid message ID.").ConfigureAwait(false);
            return;
          }
          else
          {
            m1 = await channel.GetMessageAsync(messageIDParsed1).ConfigureAwait(false);
          }
        }
        else
        {
          await Context.Channel.SendMessageAsync("Please enter a response.").ConfigureAwait(false);
          return;
        }

        await Context.Channel.SendMessageAsync("Message registered!").ConfigureAwait(false);
        await guilds.AddReactiveRole(Context.Guild.Id, channel.Id, m1.Id, role, emoji).ConfigureAwait(false);
          await m1.AddReactionAsync(flag3 ? emojiID : emoteID).ConfigureAwait(false);
      }

      await Context.Channel.SendMessageAsync("Provide the text to send.").ConfigureAwait(false);
      var mr2 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
      if (mr2.IsSuccess)
      {
        IMessage m2 = await channel.SendMessageAsync(mr2.Value.Content).ConfigureAwait(false);
          await m2.AddReactionAsync(flag3 ? emojiID : emoteID).ConfigureAwait(false);
        await guilds.AddReactiveRole(Context.Guild.Id, channel.Id, m2.Id, role, emoji).ConfigureAwait(false);
      }
      else
      {
        await Context.Channel.SendMessageAsync("Timed Out").ConfigureAwait(false);
        return;
      }
    }
    [Command("react")]
    public async Task React([Remainder] string query = null)
    {
      Emote emote = Emote.Parse("<:crystalSad:928205182855176232>");
      SocketUserMessage message = Context.Message as SocketUserMessage;
      await message.AddReactionAsync(emote).ConfigureAwait(false);
      int a;
    }
  }
}

