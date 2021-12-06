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
    public RoleModule(Guilds _guilds)
    {
      guilds = _guilds;
    }

    [Command("roles")]
    [RequireOwner]
    public async Task Role()
    {
      bool makePost = true;

      await Context.Channel.SendMessageAsync("Would you like to...\n`1`. Provide a preexisting message ID.\n`2`. Let me handle the post.").ConfigureAwait(false);

      var mpr = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

      if (mpr.IsSuccess)
      {
        if (mpr.Value.Content == "1")
          makePost = false;
        if (mpr.Value.Content == "2")
          makePost = true;
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
        return;
      }

      IMessageChannel channel1 = null;

      if (!makePost)
      {
        await Context.Channel.SendMessageAsync("Please mention the channel which the message resides in.").ConfigureAwait(false);

        var cr1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

        if (cr1.IsSuccess)
        {
          if (!TryParseChannel(cr1.Value.Content, out ulong channelID1))
          {
            await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
            return;
          }
          channel1 = await Context.Guild.GetChannelAsync(channelID1).ConfigureAwait(false) as IMessageChannel;
        }
        else
        {
          await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
          return;
        }

        await Context.Channel.SendMessageAsync("Please provide the ID of the message.").ConfigureAwait(false);

        var mr1 = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

        IMessage message1 = null;

        if (cr1.IsSuccess)
        {
          if (!ulong.TryParse(cr1.Value.Content, out ulong messageIDParsed1))
          {
            await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
            return;
          }
          if (await channel1.GetMessageAsync(messageIDParsed1).ConfigureAwait(false) == null)
          {
            await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
            return;
          }
          else
          {
            message1 = await channel1.GetMessageAsync(messageIDParsed1).ConfigureAwait(false);
          }
        }
        else
        {
          await Context.Channel.SendMessageAsync("Please enter a valid response.").ConfigureAwait(false);
          return;
        }

        await guilds.AddReactiveMessage(Context.Guild.Id, channel1.Id, message1.Id).ConfigureAwait(false);

        await Context.Channel.SendMessageAsync("Message registered!").ConfigureAwait(false);
        return;
      }

      await Context.Channel.SendMessageAsync("Would you like to...\n`1`. Provide a preexisting message ID.\n`2`. Let me handle the post.").ConfigureAwait(false);

      var makePostResult = await DiscordService.interactivity.NextMessageAsync(x => (x.Author.Id == Context.User.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Channel.Id == Context.Channel.Id) && (x.Content != string.Empty), null, TimeSpan.FromSeconds(120)).ConfigureAwait(false);
    }
  }
}
