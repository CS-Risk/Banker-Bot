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

		[Command("SpendGold")]
		public async Task SpendGold(decimal gold, string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, gold: gold.ToString()));

			// Update Sheet
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.Execute();

			// Reply in Discord
			await ReplyAsync(GetCharacterName(user) + " spent " + gold.ToString() + " gp. (" + note + ")");
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
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();

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
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s gold value changed by {1}. {2}", character, gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}
	}
}
