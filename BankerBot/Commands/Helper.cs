﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
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
		public Helper(SheetsService sheets, CalendarService calendar)
		{
			_sheetsService = sheets;
			_calendarService = calendar;
		}

		[Command("Logbook")]
		public async Task Logbook()
		{
			await ReplyAsync("Godfall Logbook: <" + ConfigurationManager.AppSettings["logbookUrl"] + ">");
		}

		[Command("Calendar")]
		[Alias("Cal")]
		public async Task Calendar(int maxResults = 3)
		{
			var eb = new EmbedBuilder();
			eb.WithTitle("Godfall Calendar");
			eb.WithUrl(ConfigurationManager.AppSettings["calendarUrl"]);
			eb.WithThumbnailUrl("https://collegeinfogeek.com/wp-content/uploads/2016/08/Google_Calendar_Logo.png");

			EventsResource.ListRequest request = _calendarService.Events.List("94cthr29rao5qahdmho975t8n0@group.calendar.google.com");
			request.TimeMin = DateTime.Now;
			request.ShowDeleted = false;
			request.SingleEvents = true;
			request.MaxResults = maxResults;
			request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

			//List Events
			Events events = request.Execute();
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					var efb = new EmbedFieldBuilder();
					efb.Name = eventItem.Summary;
					efb.Value = eventItem.Start.DateTime != null ? eventItem.Start.DateTime.ToString() : eventItem.Start.Date;
					eb.AddField(efb);
				}
			}

			await ReplyAsync("", false, eb.Build());
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
		[Alias("Date")]
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

		[Command("MoonCycle")]
		[Alias("MoonPhase")]
		public async Task MoonCycle()
		{

			await ReplyAsync(@"__**Caliban's Cycle**__
Swell - 0:00:    <:new_caliban_moon:558773491738869770>
Swell - 18:00:   <:first_quarter_caliban_moon:558773491802046464>
Crown - 12:00: <:full_caliban_moon:558773492296974346>
Ebb - 6:00:        <:last_quarter_caliban_moon:558773491302924309>
Ebb - 24:00:     <:new_caliban_moon:558773491738869770>
<:praise:558779115965120512> <http://godfall.azurewebsites.net/world/solace/calendar/>");

		}

		[Command("ImageTest")]
		public async Task Embed()
		{
			var FullMoon = new EmbedBuilder();
			var DiscordGray = new Discord.Color(0x36393F);
			FullMoon.WithImageUrl("https://i.imgur.com/Tg3b9H3.png");
			FullMoon.WithDescription("http://godfall.azurewebsites.net/world/solace/calendar/");
			FullMoon.WithColor(DiscordGray);
			await ReplyAsync("", false, FullMoon.Build());
		}
	}
}
