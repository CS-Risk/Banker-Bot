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

        [Command("OteraDate")]
        public async Task OteraDate()
        {
            DateTime OteraDate = DateTime.Now.AddYears(-1019);
            string DateString = OteraDate.ToLongDateString();

            DateString = DateString.Replace("January", "Tressym");
            DateString = DateString.Replace("February", "Auroch");
            DateString = DateString.Replace("March", "Manticore");
            DateString = DateString.Replace("April", "Almiraj");
            DateString = DateString.Replace("May", "Dragon");
            DateString = DateString.Replace("June", "Serpent");
            DateString = DateString.Replace("July", "Unicorn");
            DateString = DateString.Replace("August", "Hippogriff");
            DateString = DateString.Replace("September", "Girallon");
            DateString = DateString.Replace("October", "Cockatrice");
            DateString = DateString.Replace("November", "Winter Wolf");
            DateString = DateString.Replace("December", "Owlbear");

            DateString = DateString.Replace("Monday, ", "");
            DateString = DateString.Replace("Tuesday, ", "");
            DateString = DateString.Replace("Wednesday, ", "");
            DateString = DateString.Replace("Thursday, ", "");
            DateString = DateString.Replace("Friday, ", "");
            DateString = DateString.Replace("Saturday, ", "");
            DateString = DateString.Replace("Sunday, ", "");
            
            DateString = DateString.Insert(DateString.IndexOf(',') + 1, " Swell - Ebb,");
            DateString = DateString.Insert(DateString.Length, " A.G.");

            await ReplyAsync("It is currently " + DateString);
        }
	}
}
