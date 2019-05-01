using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BankerBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private readonly CommandService _commands = new CommandService();
        private readonly IServiceCollection _map = new ServiceCollection();
        private IServiceProvider _services;
        private string _token = "NDkwMTc1OTI0MjI3Mjc2ODAx.Dn1frg.d0eoR5GvOgqku778imX-AqF823g";
        private readonly char prefix = ConfigurationManager.AppSettings["prefix"].ToCharArray()[0];

        static string[] Scopes = { SheetsService.Scope.Spreadsheets, CalendarService.Scope.Calendar };
        static string ApplicationName = "Bankerbot";

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            //Discord API
            _client = new DiscordSocketClient();
            _client.Log += Logger;
            _commands.Log += Logger;
        }

        public async Task MainAsync()
        {
            // Centralize the logic for commands into a seperate method.
            await InitCommands();

            // Login and connect           
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(-1);
        }

        private async Task InitCommands()
        {
            // Google Sheets Credentials
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

			var calendarService = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});



            // Repeat this for all the service classes and other dependencies that your commands might need.
            //_map.AddSingleton());
            _map.AddSingleton(sheetsService);
			_map.AddSingleton(calendarService);

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            _services = _map.BuildServiceProvider();			

			// Either search the program and add all Module classes that can be found.
			// Module classes *must* be marked 'public' or they will be ignored.
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			// Subscribe a handler to see if a message invokes a command.
			_client.MessageReceived += HandleCommandAsync;
		}

        private async Task HandleCommandAsync(SocketMessage arg)
        {
			// Bail out if it's a System Message.
			if (!(arg is SocketUserMessage msg)) return;

			// Create a number to track where the prefix ends and the command begins
			int pos = 0;

            // Replace the '!' with whatever character
            // you want to prefix your commands with.
            if (msg.HasCharPrefix(prefix, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, rather an object stating if the command executed succesfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

				// Uncomment the following lines if you want the bot
				// to send a message if it failed (not advised for most situations).	
				if (result.Error != null)
					switch (result.Error)
					{
						case CommandError.BadArgCount:
							await context.Channel.SendMessageAsync(
								"Parameter count does not match any command's.");
							break;
						case CommandError.UnknownCommand:
							if (msg.Content != prefix.ToString())
							await context.Channel.SendMessageAsync(
								"Command not recognized.");
							break;
						default:
							await context.Channel.SendMessageAsync(
								$"An error has occurred {result.ErrorReason}");
							break;
					}

                if (result.IsSuccess && AppSettings.Get<bool>("DeleteCommand"))
                {
                    await context.Message.DeleteAsync();
                }
            }
        }

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        private static Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }
    }
}
