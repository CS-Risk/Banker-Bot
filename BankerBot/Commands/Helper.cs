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
	public class Helper : BankerModuleBase
	{
		public Helper(SheetsService sheets)
		{
			_sheetsService = sheets;
		}

		[Command("Logbook")]
		public async Task Logbook()
		{			
			await ReplyAsync(_sheetsService.BaseUri);
		}

	}
}
