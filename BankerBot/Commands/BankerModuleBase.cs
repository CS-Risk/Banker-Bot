using Discord;
using Discord.Commands;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Google.Apis.Calendar.v3;

namespace BankerBot.Commands
{
	public class BankerModuleBase : ModuleBase
	{
		protected readonly string _spreadsheetId = ConfigurationManager.AppSettings["spreadsheetId"];
		protected const string _logBookRange = "'Logbook'!A2:K";
		protected const string _factionRange = "'Faction Challenge'!A2:E";
		protected const string _characterRecordRange = "'Character Record'!A2:G";
		protected const string _characterListRange = "'Character List'!A2:O";

		protected SheetsService _sheetsService;
		protected CalendarService _calendarService;

		protected async Task<IList<Object>> CreateRow(IGuildUser user, string charcterName = "", string tier = "", string checkpoints = "", string lootpoints = "", string gold = "", string essence = "", string scrap = "", string note = "", string resurected = "", bool validateName = true)
		{

			IList<Object> obj = new List<Object>();

			// Check that the character exists if validateName = true (default).
			if (validateName)
			{
				if (!string.IsNullOrWhiteSpace(charcterName))
				{
					await CheckCharacterName(charcterName);
				}
				else
				{
					charcterName = GetCharacterName(user);
					await CheckCharacterName(charcterName);
				}
			}

			obj.Add(charcterName);
			obj.Add("BankerBot (" + user.Username + ")"); // DM
			obj.Add(DateTime.Today.ToShortDateString()); // Date
			obj.Add(tier); // Tier
			obj.Add(checkpoints); // Checkpoints
			obj.Add(lootpoints); // Lootpoints
			obj.Add(gold); // Gold
			obj.Add(essence); //Essence
			obj.Add(scrap); // Scrap
			obj.Add(note); // Notes
			obj.Add(resurected); // Resurected

			return obj;
		}

		protected IList<Object> CreateFactionRow(IGuildUser user, string charcterName = "", string faction = "", string date = "", string points = "", string note = "")
		{

			IList<Object> obj = new List<Object>();

			if (!string.IsNullOrWhiteSpace(charcterName)) // Character
				obj.Add(charcterName);
			else
				obj.Add(GetCharacterName(user));
			obj.Add(faction);
			obj.Add(DateTime.Today.ToShortDateString()); // Date
			obj.Add(points); // points
			obj.Add(note); // Notes

			return obj;
		}

		protected string GetCharacterName(IGuildUser user)
		{
			return GetCharacterName(user.Nickname);
		}

		protected string GetCharacterName(string nickname)
		{
			return Regex.Match(nickname, @"\(([^)]*)\)").Groups[1].Value;
		}

		protected async Task CheckCharacterName(string characterName)
		{
			// Read from Sheet
			SpreadsheetsResource.ValuesResource.GetRequest request =
				   _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _characterRecordRange);

			ValueRange response = await request.ExecuteAsync();
			IList<IList<Object>> values = response.Values;

			var row = values.FirstOrDefault(x => (string)x[0] == characterName);

			if (row == null)
			{
				throw new Exception(string.Format("A character with the name of '{0}' could not be found in the logbook.", characterName));
			}

			return;
		}

		protected async Task<ValueRange> GetCharacterRecordValueRange()
		{
			//Read from the Sheet
			SpreadsheetsResource.ValuesResource.GetRequest request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _characterRecordRange);
			request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
			request.DateTimeRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum.FORMATTEDSTRING;

			return await request.ExecuteAsync();
		}

		protected async Task ReplyWithCharacterRecordField(string characterName, int columnIndex)
		{
			// Read from Sheet
			ValueRange response = await GetCharacterRecordValueRange();

			//Find the row where 1st column matches our characterName
			var row = response.Values.FirstOrDefault(x => (string)x[0] == characterName);

			if (row == null) throw new CharacterNotFoundException(characterName);

			string valueName =
				columnIndex == Convert.ToInt32(ConfigurationManager.AppSettings["GoldColumn"]) ? "gp" :
				columnIndex == Convert.ToInt32(ConfigurationManager.AppSettings["CheckpointColumn"]) ? "ECP" :
				columnIndex == Convert.ToInt32(ConfigurationManager.AppSettings["LootpointColumn"]) ? "Lootpoints" :
				columnIndex == Convert.ToInt32(ConfigurationManager.AppSettings["EssenceColumn"]) ? "Essence" :
				columnIndex == Convert.ToInt32(ConfigurationManager.AppSettings["ScrapColumn"]) ? "scrap" :
				"";

			await ReplyAsync(String.Format("{0} has {1} {2}.", characterName, row[columnIndex].ToString(), valueName));
		}


		public string GetNewRange(string range)
		{
			// Define request parameters.
			SpreadsheetsResource.ValuesResource.GetRequest getRequest =
				_sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

			ValueRange getResponse = getRequest.Execute();
			IList<IList<Object>> getValues = getResponse.Values;

			if (getValues == null) return range;

			// Get new range
			int currentCount = getValues.Count() + 1;
			//String newRange = "'Logbook'!A" + currentCount + ":K";
			string pattern = @"^('\w*'!\w)([1-9]+)(:\w*$)";
			String newRange = Regex.Replace(range, pattern, m => m.Groups[1].Value + currentCount + m.Groups[3].Value);
			return newRange;
		}

		public async Task UpdateSheet(IList<IList<Object>> newRecords, string range = _logBookRange)
		{
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange(range));
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = await request.ExecuteAsync();
		}

		protected void DMOnly()
		{
			var user = (IGuildUser)Context.Message.Author;
			var roles = Context.Guild.Roles.Where(x => x.Name == "DM" || x.Name == "Moderator").Select(x => x.Id);
			//if (!user.RoleIds.Contains(dmRole.Id))
			if (!user.RoleIds.Any(r => roles.Contains(r)))
			{
				throw new Exception("You do not have permission to do this.");
			}
		}
	}
}
