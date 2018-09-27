﻿using Discord;
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
		protected readonly string _logBookRange = "'Logbook'!A2:J";
		protected readonly string _characterRecordRange = "'Character Record'!A2:G";
		protected readonly string _characterListRange = "'Character List'!A2:O";

		protected SheetsService _sheetsService;

		protected IList<Object> CreateRow(IGuildUser user, string charcterName = "", string tier = "", string checkpoints = "", string lootpoints = "", string gold = "", string essence = "", string scrap ="", string note = "", string resurected = "")
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

		protected string GetCharacterName(IGuildUser user)
		{
			return GetCharacterName(user.Nickname);
		}

		protected string GetCharacterName(string nickname)
		{
			return Regex.Match(nickname, @"\(([^)]*)\)").Groups[1].Value;
		}

		public string GetNewRange(SheetsService sheetsService)
		{
			// Define request parameters.

			SpreadsheetsResource.ValuesResource.GetRequest getRequest =
				sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _logBookRange);

			ValueRange getResponse = getRequest.Execute();
			IList<IList<Object>> getValues = getResponse.Values;

			if (getValues == null) return _logBookRange;

			// Get new range
			int currentCount = getValues.Count() + 1;
			String newRange = "'Logbook'!A" + currentCount + ":I";

			return newRange;
		}

		public string GetNewRange()
		{
			// Define request parameters.
			SpreadsheetsResource.ValuesResource.GetRequest getRequest =
				_sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _logBookRange);

			ValueRange getResponse = getRequest.Execute();
			IList<IList<Object>> getValues = getResponse.Values;

			if (getValues == null) return _logBookRange;

			// Get new range
			int currentCount = getValues.Count() + 1;
			String newRange = "'Logbook'!A" + currentCount + ":I";

			return newRange;
		}

		public void updateSheet(IList<IList<Object>> newRecords)
		{
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();
		}

		protected void DMOnly(IGuildUser user)
		{
			
			var dmRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "DM");
			if (!user.RoleIds.Contains(dmRole.Id))
			{
				throw new Exception("You do not have permission to do this.");
			}
		}
	}
}
