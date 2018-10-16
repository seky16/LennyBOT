// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;

    [Name("BotOwner-only commands")]
    public class BotOwnerModule : LennyBase
    {
        public BotOwnerModule(DiscordSocketClient client)
        {
            this.Client = client;
        }

        private DiscordSocketClient Client { get; }

        [Command("say"), Alias("s")]
        [Remarks("Make the bot say something")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task SayCmdAsync([Remainder]string text) => this.ReplyAsync(text);

        [Command("exit", RunMode = RunMode.Async), Alias("quit")]
        [Remarks("Make the bot go sleep")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task ExitCmdAsync()
        {
            await this.ReplyAsync("Shutting down... :zzz:").ConfigureAwait(false);
            await this.Context.Client.SetStatusAsync(UserStatus.Invisible).ConfigureAwait(false);
            await this.Context.Client.StopAsync().ConfigureAwait(false);
            Environment.Exit(1);
        }

        [Command("restart", RunMode = RunMode.Async), Alias("r")]
        [Remarks("Restart bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task RestartCmdAsync()
        {
            var msg = await this.ReplyAsync("Restarting... :arrows_counterclockwise:").ConfigureAwait(false);
            await this.Client.StopAsync().ConfigureAwait(false);
            await this.Client.LogoutAsync().ConfigureAwait(false);
            await this.Client.LoginAsync(TokenType.Bot, Configuration.Load().Token).ConfigureAwait(false);
            await this.Client.StartAsync().ConfigureAwait(false);
            ////await msg.AddReactionAsync(EmojiExtensions.FromText(":white_check_mark:")).ConfigureAwait(false);
            await msg.ModifyAsync(m => m.Content = "Restarted :white_check_mark:").ConfigureAwait(false);
        }

        [Command("botnick")]
        [Remarks("Change bot's nick")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task BotNickCmdAsync([Remainder]string name)
        {
            var iguild = this.Context.Guild as IGuild;
            var self = await iguild.GetCurrentUserAsync().ConfigureAwait(false);
            await self.ModifyAsync(x => x.Nickname = name).ConfigureAwait(false);

            await this.ReplyAsync($"I changed my name to **{name}**").ConfigureAwait(false);
        }

        [Command("playing")]
        [Remarks("Set game of bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task PlayingCmdAsync([Remainder]string game) => this.Context.Client.SetGameAsync(game);

        /*[Command("streaming")]
        [Remarks("Set stream of bot")]
        [MinPermissions(AccessLevel.BotOwner)]
        public Task StreamingCmdAsync([Remainder]string game) => this.Context.Client.SetGameAsync(game, "http://twitch.tv/seky16", StreamType.Twitch);
        */
        /*[Command("clean", RunMode = RunMode.Async), Alias("delete", "purge")]
        [Remarks("delete messages")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task CleanCmdAsync(int amount = 10, SocketGuildUser user = null)
        {
            if (amount <= 0)
            {
                await this.ReplyAsync("Invalid amount").ConfigureAwait(false);
            }

            if (!(this.Context.Channel is SocketTextChannel ch))
            {
                return;
            }

            user = user ?? this.Context.Guild.CurrentUser;
            var nick = user.Nickname ?? user.Username;

            // var messages = (await ch.GetMessagesAsync().Flatten()).Where(m => m.Author.Id == user.Id);
            // messages = messages.Take(amount).Where(m => ((DateTimeOffset.Now - m.Timestamp) < TimeSpan.FromDays(14)));
            // .Skip(Math.Max(0, messages.Count() - amount)); // https://stackoverflow.com/a/3453301
            bool old;
            var lastMessage = (await ch.GetMessagesAsync().Flatten().ConfigureAwait(false))?.FirstOrDefault();
            IEnumerable<IMessage> messages = new[] { lastMessage };
            do
            {
                var newMessages = await ch.GetMessagesAsync(messages.LastOrDefault().Id, Direction.Before).Flatten().ConfigureAwait(false);
                messages = messages.Concat(newMessages);
                old = messages.Any(m => ((DateTimeOffset.Now - m.Timestamp) > TimeSpan.FromDays(14)));
            }
            while (!old);
            messages = messages.Where(m => ((DateTimeOffset.Now - m.Timestamp) < TimeSpan.FromDays(14)))
                .Where(m => m.Author.Id == user.Id).Take(amount);
            await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);
            var str = string.Empty;
            if (messages.Count() < amount)
            {
                str = "\n*(Other messages were older than two weeks so they cannot be deleted)*";
            }

            await this.ReplyAsync($"Deleted {messages.Count()} messages of user **{nick}**.{str}").ConfigureAwait(false);
        }*/
    }
}
