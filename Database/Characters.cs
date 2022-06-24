using SnowyBot.Services;
using SnowyBot.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnowyBot.Database
{
	public class Characters
	{
		private readonly CharacterContext context;
		public Characters(CharacterContext _context) => context = _context;

		public async Task AddCharacter(ulong userID, DateTime creationDate, string prefix, string name, string gender, string sex, string species, string age, string height, string weight, string orientation, string description, string avatarURL, string referenceURL)
		{
			context.Add(new Character { UserID = userID, CharacterID = $"{userID}:{await CheckHighestID(userID).ConfigureAwait(false) + 1}", CreationDate = creationDate.ToString(), Prefix = prefix, Name = name, Gender = gender, Sex = sex, Species = species, Age = age, Height = height, Weight = weight, Orientation = orientation, Description = description, AvatarURL = avatarURL, ReferenceURL = referenceURL });
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task DeleteCharacter(Character character)
		{
			context.Remove(character);
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task EditCharacter(ulong userID, string characterID, CharacterDataType type, string value)
		{
			Character character = await context.Characters
									 .AsAsyncEnumerable()
									 .Where(x => x.UserID == userID && x.CharacterID == characterID)
									 .FirstOrDefaultAsync()
									 .ConfigureAwait(false);
			if (character == null)
			{
				await LoggingGlobal.LogAsync("CHAR", Discord.LogSeverity.Error, $"Character was null when editing character.\nUser: {userID}\nCharacter: {characterID}\nEdit Type: {type}\nValue: {value}").ConfigureAwait(false);
				return;
			}
			switch (type)
			{
				case CharacterDataType.Prefix:
					character.Prefix = value;
					break;
				case CharacterDataType.Name:
					character.Name = value;
					break;
				case CharacterDataType.Gender:
					character.Gender = value;
					break;
				case CharacterDataType.Sex:
					character.Sex = value;
					break;
				case CharacterDataType.Species:
					character.Species = value;
					break;
				case CharacterDataType.Age:
					character.Age = value;
					break;
				case CharacterDataType.Height:
					character.Height = value;
					break;
				case CharacterDataType.Weight:
					character.Weight = value;
					break;
				case CharacterDataType.Orientation:
					character.Orientation = value;
					break;
				case CharacterDataType.Description:
					character.Description = value;
					break;
				case CharacterDataType.AvatarURL:
					character.AvatarURL = value;
					break;
				case CharacterDataType.ReferenceURL:
					character.ReferenceURL = value;
					break;
			}
			await context.SaveChangesAsync().ConfigureAwait(false);
		}
		public async Task<Character> HasCharPrefix(ulong userID, string message)
		{
			Character character = await context.Characters
									 .AsAsyncEnumerable()
									 .Where(x => x.UserID == userID && message.StartsWith(x.Prefix))
									 .FirstOrDefaultAsync()
									 .ConfigureAwait(false);
			return character;
		}
		public async Task<Character> CheckPrefixExists(ulong userID, string prefix)
		{
			return await context.Characters.AsAsyncEnumerable().Where(x => x.UserID == userID && x.Prefix == prefix).FirstOrDefaultAsync();
		}
		public async Task<Character> ViewCharacterByName(ulong userID, string name)
		{
			return await context.Characters
									 .AsAsyncEnumerable()
									 .Where(x => x.UserID == userID && x.Name == name)
									 .FirstOrDefaultAsync()
									 .ConfigureAwait(false);
		}
		public async Task<Character> ViewCharacterByID(ulong userID, string characterID)
		{
			return await context.Characters
									 .AsAsyncEnumerable()
									 .Where(x => x.UserID == userID && x.CharacterID == characterID)
									 .FirstOrDefaultAsync()
									 .ConfigureAwait(false);
		}
		public async Task<ulong> CheckHighestID(ulong userID)
		{
			ulong count = 0;
			string[] characterID;
			foreach (Character character in context.Characters)
			{
				characterID = character.CharacterID.Split(":");
				if (character.UserID == userID && ulong.Parse(characterID[1]) > count)
					count = ulong.Parse(characterID[1]);
			}
			if (count == 0)
				return await Task.FromResult(0u).ConfigureAwait(false);
			return await Task.FromResult(count).ConfigureAwait(false);
		}
		public async Task<List<Character>> ListCharacters(ulong userID)
		{
			return await context.Characters
						 .AsAsyncEnumerable()
						 .Where(x => x.UserID == userID)
						 .ToListAsync()
						 .ConfigureAwait(false);
		}
	}
}
