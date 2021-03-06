﻿using Discord;
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
using Newtonsoft.Json;

namespace BankerBot.Commands
{
	public class Economy : BankerModuleBase
	{

        //Column Index on the Character Record tab
        private readonly int columnIndex = Convert.ToInt32(ConfigurationManager.AppSettings["GoldColumn"]);

        public Economy(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Gold")]
		public async Task CurrentGold()
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
			await CurrentGold(GetCharacterName(user));
		}

		[Command("Gold")]
		public async Task CurrentGold(SocketGuildUser user)
		{
			await CurrentGold(GetCharacterName(user));
		}

		[Command("Gold")]
		public async Task CurrentGold(string characterName)
		{
			await ReplyWithCharacterRecordField(characterName, columnIndex);
		}			

		[Command("SpendGold")]
		public async Task SpendGold(decimal amount, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
            newRecords.Add(await CreateRow(user, gold: negativeAmount.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} spent {1} gp. {2}", GetCharacterName(user), Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("GiveGold")]
		public async Task GiveGold(SocketGuildUser recipient, decimal gold, [Remainder]string note = "")
		{
			await GiveGold(GetCharacterName(recipient.Nickname), gold, note);
		}

		[Command("GiveGold")]
		public async Task GiveGold(string recipient, decimal amount, [Remainder]string note = "")
		{
			// Get User
			var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

            await CheckCharacterName(recipient); //Check that the Recipient exists before doing anything else.

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();

			// Create disbursing record
			newRecords.Add(await CreateRow(user, gold: negativeAmount.ToString(), note: note));

			// Create receiving record
			newRecords.Add(await CreateRow(user, charcterName: recipient, gold: Math.Abs(amount).ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0} gave {1} {2} gp. {3}", GetCharacterName(user), recipient, Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(SocketGuildUser user, decimal gold, [Remainder]string note = "")
		{
			await UpdateGold(GetCharacterName(user.Nickname), gold, note);
		}

		[Command("UpdateGold")]
		public async Task UpdateGold(string character, decimal gold, [Remainder]string note = "")
		{
			var user = (IGuildUser)Context.Message.Author;

			DMOnly();

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(await CreateRow(user, charcterName: character, gold: gold.ToString(), note: note));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s gold value changed by {1}. {2}", character, gold.ToString(), (!string.IsNullOrWhiteSpace(note) ? string.Format("({0})", note) : "")));
		}

		[Command("StartingGold")]
		public async Task StartingGold(decimal gold)
		{
			var user = (IGuildUser)Context.Message.Author;
			await StartingGold(GetCharacterName(user), gold);
		}

		[Command("StartingGold")]
		public async Task StartingGold(SocketGuildUser user, decimal gold)
		{
			await StartingGold(GetCharacterName(user), gold);
		}

		[Command("StartingGold")]
		public async Task StartingGold(string character, decimal gold)
		{
			var user = (IGuildUser)Context.Message.Author;

			if (gold > 250)
				throw new Exception(string.Format("Starting gold can not be above 250 gp."));

			// Create record
			List<IList<Object>> newRecords = new List<IList<Object>>();
			newRecords.Add(await CreateRow(user, charcterName: character, gold: gold.ToString(), note: "Starting Gold", validateName: false));

			// Update Sheet
			await UpdateSheet(newRecords);

			// Reply in Discord
			await ReplyAsync(string.Format("{0}'s starting gold set to {1} gp.", character, gold.ToString()));
		}
	}
}
