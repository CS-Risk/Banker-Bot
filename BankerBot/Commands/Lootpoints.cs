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
	public class Lootpoints : BankerModuleBase
	{
        //Column Index on the Character Record tab
        private readonly int columnIndex = Convert.ToInt32(ConfigurationManager.AppSettings["LootpointColumn"]);

        public Lootpoints(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Lootpoints")]
		[Alias("LP")]
		public async Task CurrentLootpoints()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await CurrentLootpoints(GetCharacterName(user));
		}

		[Command("Lootpoints")]
		[Alias("LP")]
		public async Task CurrentLootpoints(SocketGuildUser user)
		{
			await CurrentLootpoints(GetCharacterName(user));
		}

		[Command("Lootpoints")]
		[Alias("LP")]
		public async Task CurrentLootpoints(string characterName)
		{
			await ReplyWithCharacterRecordField(characterName, columnIndex);
		}		

		[Command("SpendLootpoints")]
		[Alias("SpendLP")]
		public async Task SpendLootpoints(decimal amount, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>
            {
                await CreateRow(user, lootpoints: negativeAmount.ToString(), note: note)
            };

            // Update Sheet
            await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} Lootpoints. {2}", GetCharacterName(user), Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
            await CurrentLootpoints();
        }

		[Command("UpdateLootpoints")]
		[Alias("UpdateLP")]
		public async Task UpdateLootpoints(SocketGuildUser user, int lootpoints, [Remainder]string note = "")
		{
			await UpdateLootpoints(GetCharacterName(user.Nickname), lootpoints, note);
		}

		[Command("UpdateLootpoints")]
		[Alias("UpdateLP")]
		public async Task UpdateLootpoints(string character, int lootpoints, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly();

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(await CreateRow(user, charcterName: character, lootpoints: lootpoints.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s Lootpoints changed by {1}. {2}", character, lootpoints.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

	}
}
