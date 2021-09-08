using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExampleBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();
        // Creating the necessary variables
        public static DiscordSocketClient _client; 
        private CommandService _commands;
        private IServiceProvider _services;

        private BotConfig config;

        // Runbot task
        public async Task RunBot()
        {
            _client = new DiscordSocketClient(); // Define _client
            _commands = new CommandService(); // Define _commands

            _services = new ServiceCollection() // Define _services
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // If there isn't a bot config, create one
            if (!File.Exists("config.json"))
            {
                config = new BotConfig()
                {
                    prefix = "!",
                    token = "",
                    game = ""
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            }

            string botToken = config.token; // Make a string for the token

            _client.Log += Log; // Logging

            await RegisterCommandsAsync(); // Call registercommands

            await _client.LoginAsync(TokenType.Bot, botToken); // Log into the bot user

            await _client.StartAsync(); // Start the bot user

            await _client.SetGameAsync(config.game); // Set the game the bot is playing

            await Task.Delay(-1); // Delay for -1 to keep the console window opened
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync; // Messagerecieved

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null); // Add module to _commands
        }

        private Task Log(LogMessage arg) // Logging
        {
            Console.WriteLine(arg); // Print the log to Console
            return Task.CompletedTask; // Return with completedtask
        }
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            string messageLower = arg.Content.ToLower(); // Convert the message to a Lower
            var message = arg as SocketUserMessage; // Create a variable with the message as SocketUserMessage
            if (message is null || message.Author.IsBot) return; // Checks if the message is empty or sent by a bot
            int argumentPos = 0; // Sets the argpos to 0 (the start of the message)
            if (message.HasStringPrefix(config.prefix, ref argumentPos) || message.HasMentionPrefix(_client.CurrentUser, ref argumentPos)) // If the message has the prefix at the start or starts with someone mentioning the bot
            {
                var context = new SocketCommandContext(_client, message); // Create a variable called context
                var result = await _commands.ExecuteAsync(context, argumentPos, _services); // Create a veriable called result

                if (!result.IsSuccess) // If the result is unsuccessful
                {
                    Console.WriteLine(result.ErrorReason); // Print the error to console
                    var eb = new EmbedBuilder();
                    if (result.ErrorReason == "Sequence contains no elements")
                    {
                        eb.WithDescription("Please use /coredata by including the file");
                    }
                    else if (result.ErrorReason == "The input text has too few parameters.")
                    {
                        eb.WithDescription("Usage: /spam <Message> <Amount>");
                    }
                    else if (result.ErrorReason == "User requires guild permission ManageRoles.")
                    {
                        eb.WithDescription("Sorry, you don't have permission to do that.");
                    }
                    else if (result.ErrorReason == "Unknown command.")
                    {
                        eb.WithDescription("Unknown command.  Enter /help for a list of valid commands");
                    }
                    else
                    {
                        eb.WithDescription("Something went wrong?!");
                    }
                    await message.Channel.SendMessageAsync("", false, eb.Build());
                }
            }
        }
    }

    class BotConfig
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string game { get; set; }
    }
}