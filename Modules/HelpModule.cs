using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Services;
using System.Threading.Tasks;

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
			EmbedBuilder builder = new();
			if (command == null)
			{

				builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
				builder.WithTitle("SnowyBot Commands!");
				//builder.WithDescription("Use `!help detailed` to list all commands and their use!");
				builder.AddField("Music", "join, play, list, pause, resume, playing, seek, jump, loop, qremove, qclear, shuffle, volume, filter, eq, lyrics, artwork, stop, leave", false);
				builder.AddField("Fun", "question, 8ball, a, info, ratewaifu, jumbo, awoo, snort, inflate, scramble, formatgreentext, timestamp, kojimafy", false);
				builder.AddField("Points", "leaderboard, points", false);
				builder.AddField("Character", "char add, char view, char delete", false);
				builder.AddField("Config (Admin)", "prefix, deletemusic, welcome, goodbye, changelog, roles", false);
				builder.WithCurrentTimestamp();
				builder.WithColor(new Color(0xcc70ff));
				builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

				await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
				return;
			}

			builder = new EmbedBuilder();
			builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/930539422343106560.webp?size=512&quality=lossless");
			builder.WithCurrentTimestamp();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", (await DiscordService.client.GetUserAsync(402246856752627713).ConfigureAwait(false) as SocketUser)?.GetAvatarUrl() ?? "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");

			switch (command)
			{
				case "join":
					builder.WithTitle("Join");
					builder.WithDescription("Makes me join the voice chat.\n- !join");
					break;
				case "play":
					builder.WithTitle("Play");
					builder.WithDescription("Plays music.\n- !play <link | search>");
					break;
				case "list":
					builder.WithTitle("List");
					builder.WithDescription("Lists the current queue.\n- !list");
					break;
				case "pause":
					builder.WithTitle("Pause");
					builder.WithDescription("Pauses the current song.\n- !pause");
					break;
				case "resume":
					builder.WithTitle("Resume");
					builder.WithDescription("Resumes the current song.\n- !resume");
					break;
				case "playing":
					builder.WithTitle("Playing");
					builder.WithDescription("Displays the current song.\n- !playing");
					break;
				case "seek":
					builder.WithTitle("Seek");
					builder.WithDescription("Plays from a position in the current song.\n- !seek <hh:mm:ss>");
					break;
				case "jump":
					builder.WithTitle("Jump");
					builder.WithDescription("Jumps forward an amount in the current song.\n- !jump [-]<hh:mm:ss>");
					break;
				case "loop":
					builder.WithTitle("Loop");
					builder.WithDescription("Loops the current song.\n- !loop [amount]");
					break;
				case "qremove":
					builder.WithTitle("QRemove");
					builder.WithDescription("Removes a song or a range of songs from the queue.\n- !qremove <index> [index2]");
					break;
				case "qclear":
					builder.WithTitle("QClear");
					builder.WithDescription("Clears the queue, but does not stop the current track.\n- !qclear");
					break;
				case "shuffle":
					builder.WithTitle("Shuffle");
					builder.WithDescription("Shuffles the queue.\n- !shuffle");
					break;
				case "volume":
					builder.WithTitle("Volume");
					builder.WithDescription("Changes the volume of the player.\n- !volume <1-150>");
					break;
				case "filter":
					builder.WithTitle("Filter");
					builder.WithDescription("Applies a filter to the player.\n- !filter vibrato");
					break;
				case "eq":
					builder.WithTitle("EQ");
					builder.WithDescription("Applies an equalizer to the player.\n- !eq <normal | default>\n- !eq <bassboost | bb>");
					break;
				case "lyrics":
					builder.WithTitle("Lyrics");
					builder.WithDescription("Tries to grab the lyrics of the current track.\n- !lyrics");
					break;
				case "artwork":
					builder.WithTitle("Artwork");
					builder.WithDescription("Tries to grab the artwork of the current track.\n- !artwork");
					break;
				case "stop":
					builder.WithTitle("Stop");
					builder.WithDescription("Stops the song and clears the queue.\n- !stop");
					break;
				case "leave":
					builder.WithTitle("Leave");
					builder.WithDescription("Makes me leave the voice chat.\n- !leave");
					break;
				case "question":
					builder.WithTitle("Question");
					builder.WithDescription("Asks me a question.\n- !question <question>");
					break;
				case "8ball":
					builder.WithTitle("8Ball");
					builder.WithDescription("Asks an 8ball a question.\n- !8ball <question>");
					break;
				case "a":
					builder.WithTitle("A");
					builder.WithDescription("Screm.\n- !a <a-z>");
					break;
				case "info":
					builder.WithTitle("Info");
					builder.WithDescription("Displays the info about the current server.\n- !info");
					break;
				case "ratewaifu":
					builder.WithTitle("Ratewaifu");
					builder.WithDescription("Rates you or someone on the waifu scale.\n- !ratewaifu\n- !ratewaifu <@mention>\n-ratewaifu <text>");
					break;
				case "jumbo":
					builder.WithTitle("Jumbo");
					builder.WithDescription("Sends a large emoji.\n- !jumbo <emoji>");
					break;
				case "awoo":
					builder.WithTitle("Awoo");
					builder.WithDescription("Awoos.\n- !awoo");
					break;
				case "snort":
					builder.WithTitle("Snort");
					builder.WithDescription("Snorts.\n- !snort");
					break;
				case "inflate":
					builder.WithTitle("Inflate");
					builder.WithDescription("Inflates someone.\n- !inflate <@mention>");
					break;
				case "scramble":
					builder.WithTitle("Scramble");
					builder.WithDescription("Scrambles a sentence, or a message you've replied to.\n- !scramble <sentence>");
					break;
				case "timestamp":
					builder.WithTitle("Timestamp");
					builder.WithDescription("Converts a time you input into a timestamp that displays the correct relative time for other users.\n- !timestamp <date | time | datetime>");
					break;
				case "kojimafy":
					builder.WithTitle("Kojimafy");
					builder.WithDescription("Kojimafy's a setnence, or a message you've replied to.\n- !kojimafy <sentence>");
					break;
				case "char add":
					builder.WithTitle("Char Add");
					builder.WithDescription("Begins the character creation process.\n- !char add");
					break;
				case "char view":
					builder.WithTitle("Char View");
					builder.WithDescription("Displays a character.\n- !char view <name>");
					break;
				case "char delete":
					builder.WithTitle("Char Dlete");
					builder.WithDescription("Deletes a character.\n- !char delete <name>");
					break;
				case "prefix":
					builder.WithTitle("Prefix [Requires Admin]");
					builder.WithDescription("Sets or displays the prefix for the current guild.\n- !prefix\n- !prefix <new prefix>");
					break;
				case "deletemusic":
					builder.WithTitle("Deletemusic [Requires Admin]");
					builder.WithDescription("Toggles the deletion of music posts for the current guild.\n- !deletemusic");
					break;
				case "welcome":
					builder.WithTitle("Welcome [Requires Admin]");
					builder.WithDescription("Sets the welcome message and channel for the current guild.\n- !welcome\n- !welcome <message>");
					break;
				case "goodbye":
					builder.WithTitle("Goodbye [Requires Admin]");
					builder.WithDescription("Sets the goodbye message and channel for the current guild.\n- !goodbye\n- !goodbye <message>");
					break;
				case "changelog":
					builder.WithTitle("Changelog [Requires Admin]");
					builder.WithDescription("Allows setting or removing a channel as the bot updates channel.\n- !changelog\n- !changelog <#channel>");
					break;
				case "roles":
					builder.WithTitle("Roles [Requires Admin]");
					builder.WithDescription("Opens the prompt for setting up Reactive Roles.\n- !roles");
					break;
			}
			if (builder.Title == null || builder.Title == string.Empty)
			{
				await Context.Channel.SendMessageAsync("Invalid command.").ConfigureAwait(false);
				return;
			}
			await Context.Channel.SendMessageAsync(null, false, builder.Build()).ConfigureAwait(false);
		}
	}
}
