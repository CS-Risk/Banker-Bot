using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BankerBot.Commands
{
	public class Economy : ModuleBase
	{
		private SheetsService _sheetsService;

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
			newRecords.Add(createRow(user, gold: gold.ToString()));

			// Update Sheet
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, Helpers.SpreadsheetId, getNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.Execute();

			// Reply in Discord
			await ReplyAsync(getCharacterName(user) + " spent " + gold.ToString() + " gp. (" + note + ")");
		}

		[Command("GiveGold")]
		public async Task GiveGold(SocketGuildUser recipient, decimal gold, string note = "")
		{
			await GiveGold(getCharacterName(recipient.Nickname), gold, note);
		}

		[Command("GiveGold")]
		public async Task GiveGold(string recipient, decimal gold, string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();

			// Create disbursing record
			newRecords.Add(createRow(user, gold: (gold * -1).ToString(), note: note));

			// Create receiving record
			newRecords.Add(createRow(user, charcterName: recipient, gold: gold.ToString(), note: note));

			// Update Sheet
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, Helpers.SpreadsheetId, getNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();

			// Reply in Discord
			await ReplyAsync(string.Format("{0} gave {1} {2} gp. {3}", getCharacterName(user), recipient, gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(SocketGuildUser character, decimal gold, string note = "")
		{
			await UpdateGold(getCharacterName(character.Nickname), gold, note);
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(string character, decimal gold, string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			var dmRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "DM");
			if (!user.RoleIds.Contains(dmRole.Id))
			{
				throw new Exception("You do not have permission to do this.");
			}

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(createRow(user, charcterName: character, gold: gold.ToString(), note: note));

			// Update Sheet
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, Helpers.SpreadsheetId, getNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s gold value changed by {1}. {2}", character, gold.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		#region Helpers

		private IList<Object> createRow(IGuildUser user, string charcterName = "", string tier = "", string checkpoints = "", string lootpoints = "", string gold = "", string note = "", string resurected = "")
		{

			IList<Object> obj = new List<Object>();

			if (!string.IsNullOrWhiteSpace(charcterName)) // Character
				obj.Add(charcterName);
			else
				obj.Add(getCharacterName(user));

			obj.Add("BankerBot (" + user.Username + ")"); // DM
			obj.Add(DateTime.Today.ToShortDateString()); // Date
			obj.Add(tier); // Tier
			obj.Add(checkpoints); // Checkpoints
			obj.Add(lootpoints); // Lootpoints
			obj.Add(gold); // Gold
			obj.Add(note); // Notes
			obj.Add(resurected); // Resurected

			return obj;
		}

		private string getCharacterName(IGuildUser user)
		{
			return getCharacterName(user.Nickname);
		}

		private string getCharacterName(string nickname)
		{
			return Regex.Match(nickname, @"\(([^)]*)\)").Groups[1].Value;
		}

		private string getNewRange()
		{
			// Define request parameters.

			SpreadsheetsResource.ValuesResource.GetRequest getRequest =
				_sheetsService.Spreadsheets.Values.Get(Helpers.SpreadsheetId, Helpers.LogBookRange);

			ValueRange getResponse = getRequest.Execute();
			IList<IList<Object>> getValues = getResponse.Values;

			if (getValues == null) return Helpers.LogBookRange;

			// Get new range
			int currentCount = getValues.Count() + 1;
			String newRange = "'Logbook'!A" + currentCount + ":I";

			return newRange;
		}

		#endregion
	}
}
