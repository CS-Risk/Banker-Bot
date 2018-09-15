using Discord;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankerBot.Commands
{
	class Helpers
	{
		public const String SpreadsheetId = "17cmOpZfy68x43jgGGHLah1_vhzAPUx1RdjixVXB8Pzs";
		public const String LogBookRange = "'Logbook'!A2:I";

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

		private string getNewRange(SheetsService sheetsService)
		{
			// Define request parameters.

			SpreadsheetsResource.ValuesResource.GetRequest getRequest =
				sheetsService.Spreadsheets.Values.Get(SpreadsheetId, LogBookRange);

			ValueRange getResponse = getRequest.Execute();
			IList<IList<Object>> getValues = getResponse.Values;

			if (getValues == null) return LogBookRange;

			// Get new range
			int currentCount = getValues.Count() + 1;
			String newRange = "'Logbook'!A" + currentCount + ":I";

			return newRange;
		}
	}
}
