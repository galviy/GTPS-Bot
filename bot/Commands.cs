using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBot
{

    class Http
    {
        public static byte[] Post(string uri, NameValueCollection pairs)
        {
            using (WebClient webClient = new WebClient())
                return webClient.UploadValues(uri, pairs);
        }

    }
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        private async Task help()
        {
            var eb = new EmbedBuilder()
                 .WithUrl("https://discord.gg/TtqPm46TV5")
                 .WithTitle("GTPS Bot")
                 .WithDescription("/help\n/coredata (items.dat to coredata)\n/ping\n")
                 .WithCurrentTimestamp();
            await ReplyAsync("", false, eb.Build());
            var ebs = new EmbedBuilder()
               .WithTitle("Moderator Command")
               .WithDescription("/purge <Amount>\n/spam <Message> <Amount(Max = 3)>")
               .WithCurrentTimestamp();
            await ReplyAsync("", false, ebs.Build());
        }
        [Command("coredata")]
        private async Task coredata()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(Context.Message.Attachments.First().Url, "items.dat");
            }
            Process.Start("Builder.exe");
            await Context.Channel.SendFileAsync("CoreData.txt", "Here you go!");
            var YourEmoji = new Emoji("😀");
            Context.Message.AddReactionAsync(YourEmoji);
        }
        [Command("ping")]
        private async Task Ping()
        {
            string Message = "Pong! 🏓 **" + Program._client.Latency + "ms**";
            var eb = new EmbedBuilder();
            eb.WithDescription(Message);
            await ReplyAsync("", false, eb.Build());
        }
        [Command("spam")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task spam(string Message, int amount)
        {
            var guildUser = Context.User as SocketGuildUser;

            if (Message.Contains("@everyone") || Message.Contains("@here"))
            {
                await ReplyAsync("You can't do that.");
                return;
            }
            else if (amount <= 0 || amount >= 4)
            {
                await ReplyAsync("Should be between 1-3.");
                return;
            }
            else
            {
                for (int i = 0; i < amount; i++) await ReplyAsync(Message);
            }
        }
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task purge(int amount)
        {
            // Check if the amount provided by the user is positive.
            if (amount <= 0)
            {
                await ReplyAsync("The amount of messages to remove must be positive.");
                return;
            }

            // Download X messages starting from Context.Message, which means
            // that it won't delete the message used to invoke this command.
            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            // Note:
            // FlattenAsync() might show up as a compiler error, because it's
            // named differently on stable and nightly versions of Discord.Net.
            // - Discord.Net 1.x: Flatten()
            // - Discord.Net 2.x: FlattenAsync()

            // Ensure that the messages aren't older than 14 days,
            // because trying to bulk delete messages older than that
            // will result in a bad request.
            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

            // Get the total amount of messages.
            var count = filteredMessages.Count();

            // Check if there are any messages to delete.
            if (count == 0)
                await ReplyAsync("Nothing to delete.");

            else
            {
                // The cast here isn't needed if you're using Discord.Net 1.x,
                // but I'd recommend leaving it as it's what's required on 2.x, so
                // if you decide to update you won't have to change this line.
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                await ReplyAsync($"Done. Removed {count} {(count > 1 ? "messages" : "message")}.");
                var YourEmoji = new Emoji("😀");
                Context.Message.AddReactionAsync(YourEmoji);
            }
        }
    }
}
