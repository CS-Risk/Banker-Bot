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
	public class Economy : BankerModuleBase
	{

		public Economy(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Gold")]
		public async Task Gold(string characterName)
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
			await ReplyAsync(String.Format("{0} has {1} gp.", characterName, (string)row[3]));
		}

		[Command("Gold")]
		public async Task Gold()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await Gold(GetCharacterName(user));
		}

		[Command("Gold")]
		public async Task Gold(SocketGuildUser user)
		{			
			await Gold(GetCharacterName(user));
		}

		[Command("SpendGold")]
		public async Task SpendGold(decimal gold, string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, gold: gold.ToString()));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} gp. {2}", GetCharacterName(user), gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("GiveGold")]
		public async Task GiveGold(SocketGuildUser recipient, decimal gold, string note = "")
		{
			await GiveGold(GetCharacterName(recipient.Nickname), gold, note);
		}

		[Command("GiveGold")]
		public async Task GiveGold(string recipient, decimal gold, string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();

			// Create disbursing record
			newRecords.Add(CreateRow(user, gold: (gold * -1).ToString(), note: note));

			// Create receiving record
			newRecords.Add(CreateRow(user, charcterName: recipient, gold: gold.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} gave {1} {2} gp. {3}", GetCharacterName(user), recipient, gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(SocketGuildUser character, decimal gold, string note = "")
		{
			await UpdateGold(GetCharacterName(character.Nickname), gold, note);
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(string character, decimal gold, string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly(user);

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, gold: gold.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s gold value changed by {1}. {2}", character, gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("StartingGold")]
		public async Task StartingGold(decimal gold)
		{
			var user = (IGuildUser)Context.Message.Author;
			await StartingGold(GetCharacterName(user), gold);
		}

		[Command("StartingGold")]
		public async Task StartingGold(string character, decimal gold)
		{
			var user = (IGuildUser)Context.Message.Author;

			if (gold > 250)
				throw new Exception(string.Format("Starting gold can not be above 250 gp."));

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, gold: gold.ToString(), note: "Starting Gold"));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s starting gold set to {1} gp.", character, gold.ToString()));
		}
	}
}
