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
	public class Factions : BankerModuleBase
	{
		public Factions(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Points")]
		public async Task Points()
		{
			// Read from Sheet
			SpreadsheetsResource.ValuesResource.GetRequest request =
				   _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _factionRange);

			ValueRange response = request.Execute();
			IList<IList<Object>> values = response.Values;

			var greenhands = values.Where(x => (string)x[1] == "Greenhands").Sum(x => Convert.ToInt32(x[3]));
			var framers = values.Where(x => (string)x[1] == "Framers").Sum(x => Convert.ToInt32(x[3]));
			var conservators = values.Where(x => (string)x[1] == "Conservators").Sum(x => Convert.ToInt32(x[3]));
			var wardens = values.Where(x => (string)x[1] == "Wardens").Sum(x => Convert.ToInt32(x[3]));

			await ReplyAsync(String.Format("```Greenhands: {0} \n Framers: {1} \n Conservators: {2} \n Wardens {3}```", greenhands, framers, conservators, wardens));

		}

		[Command("GreenhandsPoints")]
		public async Task GiveGreenhandsPoints(int points, [Remainder]string note = "")
		{
			await GivePoints("Greenhands", points, note);
		}

		[Command("FramersPoints")]
		public async Task GiveFramesrPoints(int points, [Remainder]string note = "")
		{
			await GivePoints("Framers", points, note);
		}

		[Command("ConservatorsPoints")]
		public async Task GiveConservatorsPoints(int points, [Remainder]string note = "")
		{
			await GivePoints("Conservators", points, note);
		}

		[Command("WardensPoints")]
		public async Task GiveWardensPoints(int points, [Remainder]string note = "")
		{
			await GivePoints("Wardens", points, note);
		}

		[Command("GivePoints")]
		public async Task GivePoints(string faction, int points, [Remainder]string note = "")
		{		
			// Get User
			var user = (IGuildUser)Context.Message.Author;
		
			if (faction != "Greenhands" && faction != "Framers" && faction != "Conservators" && faction != "Wardens") {
				throw new Exception(string.Format("{0} not reconized as a valid faction.", faction));
			}

			// Create record
				List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateFactionRow(user, charcterName: GetCharacterName(user.Nickname), faction: faction, points: points.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords, _factionRange);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} point(s) to the {1}! {2}", points.ToString(), faction, (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
;			//await ReplyAsync(string.Format("{0}'s Scrap value changed by {1}. {2}", character, Scrap.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));

		}
	}


}
