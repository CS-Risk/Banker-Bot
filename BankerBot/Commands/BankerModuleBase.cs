using Discord;
using Discord.Commands;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BankerBot.Commands
{
	public class BankerModuleBase : ModuleBase
	{
		//protected const string _spreadsheetId = "17cmOpZfy68x43jgGGHLah1_vhzAPUx1RdjixVXB8Pzs";
		protected readonly string _spreadsheetId = ConfigurationManager.AppSettings["spreadsheetId"];
		protected const string _logBookRange = "'Logbook'!A2:K";
		protected const string _factionRange = "'Faction Challenge'!A2:E";
		protected const string _characterRecordRange = "'Character Record'!A2:G";
		protected const string _characterListRange = "'Character List'!A2:O";

		protected SheetsService _sheetsService;

		protected IList<Object> CreateRow(IGuildUser user, string charcterName = "", string tier = "", string checkpoints = "", string lootpoints = "", string gold = "", string essence = "", string scrap = "", string note = "", string resurected = "")
		{

			IList<Object> obj = new List<Object>();

			if (!string.IsNullOrWhiteSpace(charcterName)) // Character
				obj.Add(charcterName);
			else
				obj.Add(GetCharacterName(user));

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


		public void updateSheet(IList<IList<Object>> newRecords, string range = _logBookRange)
		{
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange(range));
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();
		}

		protected void DMOnly()
		{
			var user = (IGuildUser)Context.Message.Author;
			var dmRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "DM");
			if (!user.RoleIds.Contains(dmRole.Id))
			{
				throw new Exception("You do not have permission to do this.");
			}
		}
	}
}
