using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Services;
using System.Text;
using System.Threading.Tasks;
using static SnowyBot.SnowyBotUtils;

namespace SnowyBot.Modules
{
  public class HelpModule : ModuleBase
  {
    [Command("Invite")]
    public async Task Invite()
    {
      await Context.Channel.SendMessageAsync("https://discord.com/api/oauth2/authorize?client_id=814780665018318878&permissions=8&scope=bot").ConfigureAwait(false);
    }
    [Command("Help")]
    public async Task Help([Remainder] string command = null)
    {
      EmbedBuilder builder = new EmbedBuilder();
      if (command == null)
      {
        builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
        builder.WithTitle("SnowyBot Commands!");
        builder.WithDescription("Use `!help detailed` to list all commands and their use!");
        builder.AddField("Music", "join, play, list, pause, resume, playing, seek, jump, loop, qremove, shuffle, volume, stop, leave", false);
        builder.AddField("Fun", "question, 8ball, a, info, ratewaifu, jumbo, awoo, snort", false);
        builder.AddField("Character", "char add, char view, char delete", false);
        builder.AddField("Config", "prefix, deletemusic, welcome, goodbye", false);
        builder.WithCurrentTimestamp();
        builder.WithColor(new Color(0xcc70ff));
        builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

        await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
        return;
      }
      if (string.Equals(command, "detailed", System.StringComparison.OrdinalIgnoreCase))
      {
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.AddField("Join", "Makes me join the voice chat.\n- !join", true);
        //builder.WithCurrentTimestamp();
        //builder.WithColor(new Color(0xcc70ff));
        //builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

        await Context.Channel.SendMessageAsync("Not implemented yet.").ConfigureAwait(false);
        return;
      }

      builder = null;

      switch (command)
      {
        case "join":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Join");
          builder.WithDescription("Makes me join the voice chat.\n- !join");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "play":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Play");
          builder.WithDescription("Plays music.\n- !play <link | search>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "list":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("List");
          builder.WithDescription("Lists the current queue.\n- !list");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "pause":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Pause");
          builder.WithDescription("Pauses the current song.\n- !pause");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "resume":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Resume");
          builder.WithDescription("Resumes the current song.\n- !resume");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "playing":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Playing");
          builder.WithDescription("Displays the current song.\n- !playing");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "seek":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Seek");
          builder.WithDescription("Plays from a position in the current song.\n- !seek <hh:mm:ss>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "jump":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Jump");
          builder.WithDescription("Jumps forward an amount in the current song.\n- !jump <hh:mm:ss>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "loop":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Loop");
          builder.WithDescription("Loops the current song.\n- !loop");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "qremove":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("QRemove");
          builder.WithDescription("Removes a song from the queue.\n- !queueremove <index>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "shuffle":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Shuffle");
          builder.WithDescription("Shuffles the queue.\n- !shuffle");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "volume":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Volume");
          builder.WithDescription("Changes the volume of the player.\n- !volume <1-150>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "stop":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Stop");
          builder.WithDescription("Stops the song and clears the queue.\n- !stop");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "leave":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Leave");
          builder.WithDescription("Makes me leave the voice chat.\n- !leave");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "question":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Question");
          builder.WithDescription("Asks me a question.\n- !question <question>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "8ball":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("8Ball");
          builder.WithDescription("Asks an 8ball a question.\n- !8ball <question>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "a":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("A");
          builder.WithDescription("Screm.\n- !a <a-z>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "info":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Info");
          builder.WithDescription("Displays the info about the current server.\n- !info");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "ratewaifu":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Ratewaifu");
          builder.WithDescription("Rates you or someone on the waifu scale.\n- !ratewaifu\n- !ratewaifu <@mention>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "jumbo":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Jumbo");
          builder.WithDescription("Sends a large emoji.\n- !jumbo <emoji>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "awoo":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Awoo");
          builder.WithDescription("Awoos.\n- !awoo");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "snort":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Snort");
          builder.WithDescription("Snorts.\n- !snort");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "char add":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Char Add");
          builder.WithDescription("Begins the character creation process.\n- !char add");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "char view":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Char View");
          builder.WithDescription("Displays a character.\n- !char view <name>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "char delete":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Char Dlete");
          builder.WithDescription("Deletes a character.\n- !char delete <name>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "prefix":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Prefix [Requires Admin]");
          builder.WithDescription("Sets or displays the prefix for the current guild.\n- !prefix\n- !prefix <new prefix>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "deletemusic":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Deletemusic [Requires Admin]");
          builder.WithDescription("Toggles the deletion of music posts for the current guild.\n- !prefix\n- !prefix <new prefix>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "welcome":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Welcome [Requires Admin]");
          builder.WithDescription("Sets the welcome message and channel for the current guild.\n- !prefix\n- !prefix <new prefix>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
        case "goodbye":
          builder = new EmbedBuilder();
          builder.WithThumbnailUrl("https://i.vgy.me/KOU3eR.png");
          builder.WithTitle("Goodbye [Requires Admin]");
          builder.WithDescription("Sets the goodbye message and channel for the current guild.\n- !prefix\n- !prefix <new prefix>");
          builder.WithCurrentTimestamp();
          builder.WithColor(new Color(0xcc70ff));
          builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
          break;
      }
      if(builder == null)
      {
        await Context.Channel.SendMessageAsync("Invalid command.").ConfigureAwait(false);
        return;
      }
      await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
    }
  }
}
