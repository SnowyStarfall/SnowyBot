using Discord;
using SnowyBot.Database;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
	public class EmbedHandler
	{
		public static Characters characters;
		public EmbedHandler(Characters _characters) => characters = _characters;

		public static async Task<Embed> CreateCharacterEmbedAsync(ulong userID, string characterID, string[] userData)
		{
			Character character = await characters.ViewCharacterByID(userID, $"{userID}:{characterID}").ConfigureAwait(false);

			EmbedBuilder builder = new();
			builder.WithAuthor($"{userData[0]}#{userData[1]}", userData[2]);
			builder.WithThumbnailUrl(character.AvatarURL);
			builder.WithTitle(character.Name);
			builder.WithDescription(character.Description);
			builder.AddField("Prefix", character.Prefix, true);
			builder.AddField("Gender", character.Gender, true);
			builder.AddField("Sex", character.Sex, true);
			builder.AddField("Species", character.Species, true);
			builder.AddField("Age", character.Age + " years", true);
			builder.AddField("Height", character.Height, true);
			builder.AddField("Weight", character.Weight, true);
			builder.AddField("Orientation", character.Orientation, true);
			builder.AddField("Created", character.CreationDate, true);
			builder.WithImageUrl(character.ReferenceURL);
			builder.WithCurrentTimestamp();
			builder.WithColor(new Color(0xcc70ff));
			builder.WithFooter("Bot created by SnowyStarfall - Snowy#0364", "https://cdn.discordapp.com/attachments/601939916728827915/903417708534706206/shady_and_crystal_vampires_cropped_for_bot.png");
			return builder.Build();
		}
		public static string NumToEmoji(int num)
		{
			return num == 0 ? "0️⃣" :
						 num == 1 ? "1️⃣" :
						 num == 2 ? "2️⃣" :
						 num == 3 ? "3️⃣" :
						 num == 4 ? "4️⃣" :
						 num == 5 ? "5️⃣" :
						 num == 6 ? "6️⃣" :
						 num == 7 ? "7️⃣" :
						 num == 8 ? "8️⃣" :
						 num == 9 ? "9️⃣" :
						 num == 10 ? "🔟" :
						 "Unknown";
		}
		public static int EmojiToNum(string emoji)
		{
			return emoji == "0️⃣" ? 0 :
						 emoji == "1️⃣" ? 1 :
						 emoji == "1️⃣" ? 2 :
						 emoji == "3️⃣" ? 3 :
						 emoji == "4️⃣" ? 4 :
						 emoji == "5️⃣" ? 5 :
						 emoji == "6️⃣" ? 6 :
						 emoji == "7️⃣" ? 7 :
						 emoji == "8️⃣" ? 8 :
						 emoji == "9️⃣" ? 9 :
						 emoji == "🔟" ? 10 :
						 -1;
		}
	}
}
