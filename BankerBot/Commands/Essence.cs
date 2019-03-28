using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Sheets.v4;
using Data = Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankerBot.Commands
{
	public class Essence : BankerModuleBase
	{
        //Column Index on the Character Record tab
        private readonly int columnIndex = Convert.ToInt32(ConfigurationManager.AppSettings["EssenceColumn"]);

        public Essence(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Essence")]
		public async Task CurrentEssence()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await CurrentEssence(GetCharacterName(user));
		}

		[Command("Essence")]
		public async Task CurrentEssence(SocketGuildUser user)
		{
			await CurrentEssence(GetCharacterName(user));
		}

		[Command("Essence")]
		public async Task CurrentEssence(string characterName)
		{
			await ReplyWithCharacterRecordField(characterName, columnIndex);
		}		

		[Command("SpendEssence")]
		public async Task SpendEssence(int amount, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(await CreateRow(user, essence: negativeAmount.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} Essence. {2}", GetCharacterName(user), Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
        }

		[Command("GiveEssence")]
		public async Task GiveEssence(SocketGuildUser recipient, int Essence, [Remainder]string note = "")
		{
			await GiveEssence(GetCharacterName(recipient.Nickname), Essence, note);
		}

		[Command("GiveEssence")]
		public async Task GiveEssence(string recipient, int Essence, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;

            await CheckCharacterName(recipient); //Check that the Recipient exists before doing anything else.

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();

			// Create disbursing record
			newRecords.Add(await CreateRow(user, essence: (Essence * -1).ToString(), note: note));

			// Create receiving record
			newRecords.Add(await CreateRow(user, charcterName: recipient, essence: Essence.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} gave {1} {2} Essence. {3}", GetCharacterName(user), recipient, Essence.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateEssence")]
		public async Task UpdateEssence(SocketGuildUser user, int Essence, [Remainder]string note = "")
		{
			await UpdateEssence(GetCharacterName(user.Nickname), Essence, note);
		}

		[Command("UpdateEssence")]
		public async Task UpdateEssence(string character, int Essence, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;
			DMOnly();

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(await CreateRow(user, charcterName: character, essence: Essence.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s Essence value changed by {1}. {2}", character, Essence.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}
	}
}
