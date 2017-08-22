using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOT.Config;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LennyBOT.Modules
{
    [Name("BotOwner-only commands")]
    public class BotOwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("say"), Alias("s")]
        [Remarks("Make the bot say something")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task SayCmd([Remainder]string text)
        {
            await ReplyAsync(text);
        }

        [Command("exit", RunMode = RunMode.Async), Alias("quit")]
        [Remarks("Make the bot go sleep")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ExitCmd()
        {
            await ReplyAsync("Shutting down... :zzz:");
            await Context.Client.SetStatusAsync(UserStatus.Invisible);
            await Context.Client.StopAsync();
            Environment.Exit(1);
        }

        [Command("restart", RunMode = RunMode.Async), Alias("r")]
        [Remarks("Restart bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task RestartCmd()
        {
            await ReplyAsync("Restarting... :arrows_counterclockwise:");
            await Context.Client.SetStatusAsync(UserStatus.Invisible);
            await Context.Client.StopAsync();
            Environment.Exit(0);
        }

        [Command("botnick")]
        [Remarks("Change bot's nick")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotNickCmd([Remainder]string name)
        {
            var iguild = Context.Guild as IGuild;
            var self = await iguild.GetCurrentUserAsync();
            await self.ModifyAsync(x => x.Nickname = name);

            await ReplyAsync($"I changed my name to **{name}**");
        }

        [Command("playing")]
        [Remarks("Set game of bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PlayingCmd([Remainder]string game)
        {
            await Context.Client.SetGameAsync(game);
        }

        [Command("streaming")]
        [Remarks("Set stream of bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task StreamingCmd([Remainder]string game)
        {
            await Context.Client.SetGameAsync(game, "http://twitch.tv/seky16", StreamType.Twitch);
        }

        [Command("clean"), Alias("delete", "purge")]
        [Remarks("delete messages")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task CleanCmd(int amount = 10, SocketGuildUser user = null)
        {
            if (amount <= 0) await ReplyAsync("Invalid amount");
            var ch = Context.Channel as SocketTextChannel;
            user = user ?? Context.Guild.CurrentUser;
            string nick = user.Nickname ?? user.Username;
            //var messages = (await ch.GetMessagesAsync().Flatten()).Where(m => m.Author.Id == user.Id);
            //messages = messages.Take(amount).Where(m => ((DateTimeOffset.Now - m.Timestamp) < TimeSpan.FromDays(14)));
            // .Skip(Math.Max(0, messages.Count() - amount)); // https://stackoverflow.com/a/3453301

            bool old = false;
            var lastMessage = (await ch.GetMessagesAsync(100).Flatten()).FirstOrDefault();
            IEnumerable<IMessage> messages = new IMessage[] { lastMessage };
            do
            {
                var newMessages = (await ch.GetMessagesAsync(messages.LastOrDefault().Id, Direction.Before).Flatten());
                messages = messages.Concat(newMessages);
                old = messages.Any(m => ((DateTimeOffset.Now - m.Timestamp) > TimeSpan.FromDays(14)));
            } while (!old);
            messages = messages.Where(m => ((DateTimeOffset.Now - m.Timestamp) < TimeSpan.FromDays(14))).Where(m => m.Author.Id == user.Id).Take(amount);
            await ch.DeleteMessagesAsync(messages);
            string str = String.Empty;
            if (messages.Count() < amount) str = "\n*(Other messages were older than two weeks so they cannot be deleted)*";
            await ReplyAsync($"Deleted {messages.Count()} messages of user **{nick}**.{str}");
        }
    }
}