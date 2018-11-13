using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankerBot.Commands
{
	public class Essence : BankerModuleBase
	{
		private readonly int rowIndex = 4;

		public Essence(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Essence")]
		public async Task CurrentEssence(string characterName)
		{
			// Read from Sheet
			SpreadsheetsResource.ValuesResource.GetRequest request =
				   _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _characterRecordRange);

			ValueRange response = request.Execute();
			IList<IList<Object>> values = response.Values;

			//Find the first row
			var row = values.FirstOrDefault(x => (string)x[0] == characterName);
			if (row == null)
			{
				throw new Exception(string.Format("A character with the name of '{0}' could not be found in the logbook.", characterName));
			}
			await ReplyAsync(String.Format("{0} has {1} Essence.", characterName, (string)row[rowIndex]));
		}

		[Command("Essence")]
		public async Task CurrentEssence()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await CurrentEssence(GetCharacterName(user));
		}

		[Command("Essence")]
		public async Task CurrentEssence(SocketGuildUser user)
		{			
			await CurrentEssence(GetCharacterName(user));
		}

		[Command("SpendEssence")]
		public async Task SpendEssence(decimal Essence, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, essence: Essence.ToString()));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} Essence. {2}", GetCharacterName(user), Essence.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("GiveEssence")]
		public async Task GiveEssence(SocketGuildUser recipient, decimal Essence, [Remainder]string note = "")
		{
			await GiveEssence(GetCharacterName(recipient.Nickname), Essence, note);
		}

		[Command("GiveEssence")]
		public async Task GiveEssence(string recipient, decimal Essence, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();

			// Create disbursing record
			newRecords.Add(CreateRow(user, essence: (Essence * -1).ToString(), note: note));

			// Create receiving record
			newRecords.Add(CreateRow(user, charcterName: recipient, essence: Essence.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} gave {1} {2} Essence. {3}", GetCharacterName(user), recipient, Essence.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateEssence")]
		public async Task UpdateEssence(SocketGuildUser user, decimal Essence, [Remainder]string note = "")
		{
			await UpdateEssence(GetCharacterName(user.Nickname), Essence, note);
		}

		[Command("UpdateEssence")]
		public async Task UpdateEssence(string character, decimal Essence, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly();

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, essence: Essence.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s Essence value changed by {1}. {2}", character, Essence.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}
	}
}
