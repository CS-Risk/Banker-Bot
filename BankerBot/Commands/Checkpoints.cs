using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankerBot.Commands
{
	public class Checkpoints : BankerModuleBase
	{
        //Column Index on the Character Record tab
        private readonly int columnIndex = Convert.ToInt32(ConfigurationManager.AppSettings["CheckpointColumn"]);

        public Checkpoints(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

        //[Command("ECP")]
        //public async Task CurrentECP()
        //{
        //    // Get User
        //    var user = (IGuildUser)Context.Message.Author;
        //    await CurrentCheckpoints(GetCharacterName(user));
        //}

        //[Command("ECP")]
        //public async Task CurrentECP(SocketGuildUser user)
        //{
        //    await CurrentCheckpoints(GetCharacterName(user));
        //}

        //[Command("ECP")]
        //public async Task CurrentECP(string characterName)
        //{
        //    await CurrentCheckpoints(characterName);
        //}

        [Command("Checkpoints")]
		[Alias("ECP")]
		public async Task CurrentCheckpoints(string characterName)
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
            await ReplyAsync(String.Format("{0} has {1} ECP.", characterName, (string)row[columnIndex]));
        }

        [Command("Checkpoints")]
		[Alias("ECP")]
		public async Task CurrentCheckpoints()
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            await CurrentCheckpoints(GetCharacterName(user));
        }

        [Command("Checkpoints")]
		[Alias("ECP")]
		public async Task CurrentCheckpoints(SocketGuildUser user)
        {
            await CurrentCheckpoints(GetCharacterName(user));
        }

        [Command("UpdateCheckpoints")]
		[Alias("UpdateECP")]
		public async Task UpdateCheckpoints(SocketGuildUser user, int checkpoints, [Remainder]string note = "")
		{
			await UpdateCheckpoints(GetCharacterName(user.Nickname), checkpoints, note);
		}

		[Command("UpdateCheckpoints")]
		[Alias("UpdateECP")]
		public async Task UpdateCheckpoints(string character, int checkpoints, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly();

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, checkpoints: checkpoints.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("Awarded {0} {1} ECP. {2}", character, checkpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("BeBetter")]
		public async Task BeBetter()
		{
			var user = (IGuildUser)Context.Message.Author;
			await BeBetter(GetCharacterName(user));
		}

		[Command("BeBetter")]
		public async Task BeBetter(SocketGuildUser user)
		{
			await BeBetter(GetCharacterName(user));
		}

		[Command("BeBetter")]
		public async Task BeBetter(string character)
		{
			var user = (IGuildUser)Context.Message.Author;
			var checkpoints = 1;
			var note = "Be Better - " + DateTime.Now.ToString("MMMM");

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(CreateRow(user, charcterName: character, checkpoints: checkpoints.ToString(), note: note));

			// Update Sheet
			updateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("Awarded {0} {1} ECP. {2}", character, checkpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}


	}
}
