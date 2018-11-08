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
	public class Lootpoints : BankerModuleBase
	{
		private readonly int columnIndex = 2;

		public Lootpoints(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Lootpoints")]
		public async Task CurrentLootpoints(string characterName)
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
			await ReplyAsync(String.Format("{0} has {1} Lootpoints.", characterName, (string)row[columnIndex]));
		}

		[Command("Lootpoints")]
		public async Task CurrentLootpoints()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await CurrentLootpoints(GetCharacterName(user));
		}

		[Command("Lootpoints")]
		public async Task CurrentLootpoints(SocketGuildUser user)
		{
			await CurrentLootpoints(GetCharacterName(user));
		}

		[Command("SpendLootpoints")]
		public async Task SpendLootpoints(decimal lootpoints, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, lootpoints: lootpoints.ToString()));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} Lootpoints. {2}", GetCharacterName(user), lootpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateLootpoints")]
		public async Task UpdateLootpoints(SocketGuildUser user, int lootpoints, [Remainder]string note = "")
		{
			await UpdateLootpoints(GetCharacterName(user.Nickname), lootpoints, note);
		}

		public async Task UpdateLootpoints(string character, int lootpoints, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly(user);

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, lootpoints: lootpoints.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s Lootpoints changed by {1}. {2}", character, lootpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

	}
}
