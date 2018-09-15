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
	public class Checkpoints : BankerModuleBase
	{
		public Checkpoints(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("GiveCheckpoints")]
		public async Task GiveCheckpoints(SocketGuildUser character, int checkpoints, string note = "")
		{
			await GiveCheckpoints(GetCharacterName(character.Nickname), checkpoints, note);
		}

		[Command("GiveCheckpoints")]
		public async Task GiveCheckpoints(string character, int checkpoints, string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly(user);

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, checkpoints: checkpoints.ToString(), note: note));

			// Update Sheet
			SpreadsheetsResource.ValuesResource.AppendRequest request =
				_sheetsService.Spreadsheets.Values.Append(new ValueRange() { Values = newRecords }, _spreadsheetId, GetNewRange());
			request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
			request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			var response = request.ExecuteAsync();

			// Reply in Discord
			await ReplyAsync(string.Format("Awarded {0} {1} checkpoint(s). {2}", character, checkpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}


	}
}
