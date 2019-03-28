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

		[Command("Checkpoints")]
		[Alias("ECP")]
		public async Task CurrentCheckpoints(string characterName)
        {
			await ReplyWithCharacterRecordField(characterName, columnIndex);
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
			newRecords.Add(await CreateRow(user, charcterName: character, checkpoints: checkpoints.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

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
			newRecords.Add(await CreateRow(user, charcterName: character, checkpoints: checkpoints.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("Awarded {0} {1} ECP. {2}", character, checkpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}


	}
}
