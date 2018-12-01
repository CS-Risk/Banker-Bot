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
    public class Scrap : BankerModuleBase
    {
        private readonly int columnIndex = 5;

        public Scrap(SheetsService sheets)
        {
            _sheetsService = sheets;
        }

        [Command("Scrap")]
        public async Task CurrentScrap(string characterName)
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
            await ReplyAsync(String.Format("{0} has {1} Scrap.", characterName, (string)row[columnIndex]));
        }

        [Command("Scrap")]
        public async Task CurrentScrap()
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            await CurrentScrap(GetCharacterName(user));
        }

        [Command("Scrap")]
        public async Task CurrentScrap(SocketGuildUser user)
        {
            await CurrentScrap(GetCharacterName(user));
        }

        [Command("SpendScrap")]
        public async Task SpendScrap(decimal amount, [Remainder]string note = "")
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();
            newRecords.Add(CreateRow(user, scrap: negativeAmount.ToString()));

            // Update Sheet
            updateSheet(newRecords);

            // Reply in Discord
            await ReplyAsync(string.Format("{0} spent {1} Scrap. {2}", GetCharacterName(user), Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
        }


        [Command("SellScrap")]
        public async Task SellScrap(decimal amount, [Remainder]string note = "")
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);
            var goldAmount = Math.Abs(amount / 2);          

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();
            newRecords.Add(CreateRow(user, scrap: negativeAmount.ToString(), gold: goldAmount.ToString()));

            // Update Sheet
            updateSheet(newRecords);

            // Read from Sheet
            SpreadsheetsResource.ValuesResource.GetRequest request =
                   _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _characterRecordRange);

            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;

            //Find the first row
            var row = values.FirstOrDefault(x => (string)x[0] == GetCharacterName(user));
            if (row == null)
            {
                throw new Exception(string.Format("A character with the name of '{0}' could not be found in the logbook.", GetCharacterName(user)));
            }

            string scrapValue = (string)row[columnIndex];
            string goldValue = (string)row[3];

            // Reply in Discord
            await ReplyAsync(string.Format("{0} sold {1} Scrap for {2} gp. {3} \nThey now have {4} scrap and {5} gp.", GetCharacterName(user), Math.Abs(amount).ToString(), goldAmount.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : ""), scrapValue, goldValue));
        }

        [Command("GiveScrap")]
        public async Task GiveScrap(SocketGuildUser recipient, decimal Scrap, [Remainder]string note = "")
        {
            await GiveScrap(GetCharacterName(recipient.Nickname), Scrap, note);
        }

        [Command("GiveScrap")]
        public async Task GiveScrap(string recipient, decimal amount, [Remainder]string note = "")
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            var negativeAmount = -Math.Abs(amount);

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();

            // Create disbursing record
            newRecords.Add(CreateRow(user, scrap: negativeAmount.ToString(), note: note));

            // Create receiving record
            newRecords.Add(CreateRow(user, charcterName: recipient, scrap: Math.Abs(amount).ToString(), note: note));

            // Update Sheet
            updateSheet(newRecords);

            // Reply in Discord
            await ReplyAsync(string.Format("{0} gave {1} {2} Scrap. {3}", GetCharacterName(user), recipient, Math.Abs(amount).ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
        }

        [Command("UpdateScrap")]
        public async Task UpdateScrap(decimal scrap, [Remainder]string note = "")
        {
            // Get User
            var user = (IGuildUser)Context.Message.Author;
            await UpdateScrap(GetCharacterName(user), scrap, note);
        }

        [Command("UpdateScrap")]
        public async Task UpdateScrap(SocketGuildUser user, decimal scrap, [Remainder]string note = "")
        {
            await UpdateScrap(GetCharacterName(user.Nickname), scrap, note);
        }

        [Command("UpdateScrap")]
        public async Task UpdateScrap(string character, decimal Scrap, [Remainder]string note = "")
        {
            var user = (IGuildUser)Context.Message.Author;

            // Create record
            List<IList<Object>> newRecords = new List<IList<Object>>();
            newRecords.Add(CreateRow(user, charcterName: character, scrap: Scrap.ToString(), note: note));

            // Update Sheet
            updateSheet(newRecords);

            // Reply in Discord
            await ReplyAsync(string.Format("{0}'s Scrap value changed by {1}. {2}", character, Scrap.ToString(), (!string.IsNullOrEmpty(note) ? string.Format("({0})", note) : "")));
        }
    }
}
