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
	public class Helper : BankerModuleBase
	{
		public Helper(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Logbook")]
		public async Task Logbook()
		{			
			await ReplyAsync("Godfall Logbook: <" + ConfigurationManager.AppSettings["logbookUrl"] +">");
		}

		[Command("Character")]
		public async Task Character()
		{
			var user = (IGuildUser)Context.Message.Author;
			await ReplyAsync(GetCharacterName(user));
		}

		[Command("Character")]
		public async Task Character(SocketGuildUser user)
		{
			await ReplyAsync(GetCharacterName(user));
		}	

        [Command("ServerTime")]
        public async Task CurrentTime()
        {
            DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Eastern Standard Time");
            await ReplyAsync("The current server time is: " + eastern.ToString("t"));
        }
	}
}
