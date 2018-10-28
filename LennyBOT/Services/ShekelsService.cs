// ReSharper disable StyleCop.SA1600
// ReSharper disable UsePatternMatching
namespace LennyBOT.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Extensions;
    using LennyBOT.Models;

    public class ShekelsService
    {
        //// todo: complete rework

        private readonly List<Player> players;

        public ShekelsService()
        {
            this.players = new List<Player>();
        }

        public SocketCommandContext Context { private get; set; }

        /*public List<Player> Players
        {
            get
            {
                this.DownloadAsync();
                return this.players;
            }
        }*/

        public async Task<List<Player>> DownloadAsync()
        {
            var channel = this.Context.Guild.Channels.FirstOrDefault(ch => ch.Name.ToLowerInvariant() == "shekels") as SocketTextChannel;
            if (channel == null)
            {
                await this.Context.Message.AddReactionAsync(EmojiExtensions.FromText("no_entry"));
                return null;
            }

            var msg = await channel.GetLastMessageAsync();
            var str = msg.Content;
            var sr = new StringReader(str);
            string s;
            while ((s = await sr.ReadLineAsync()) != null)
            {
                var split = s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                // string[] split = s.Split(new Char[] {';', ' '});
                var id = Convert.ToUInt64(split[0]);
                var shekels = Convert.ToInt32(split[1]);
                var p = new Player(id, shekels);
                this.AddOrUpdatePlayer(p);
            }

            return this.players;
        }

        public Task<Player> GetPlayerAsync(IUser user) => this.GetPlayerAsync(user.Id);

        public void AddOrUpdatePlayer(Player player)
        {
            var added = false;
            foreach (var p in this.players)
            {
                if (p.Id != player.Id)
                {
                    continue;
                }

                p.Shekels = player.Shekels;
                added = true;
                break;
            }

            if (added == false)
            {
                this.players.Add(player);
            }
        }

        public async Task UploadAsync()
        {
            var channel = this.Context.Guild.Channels.FirstOrDefault(ch => ch.Name.ToLowerInvariant() == "shekels") as SocketTextChannel;
            if (channel == null)
            {
                await this.Context.Message.AddReactionAsync(EmojiExtensions.FromText("no_entry"));
                return;
            }

            var msg = await channel.GetLastMessageAsync() as SocketUserMessage;

            var stringBuilder = new StringBuilder();
            foreach (var player in this.players)
            {
                stringBuilder.AppendLine($"{player.Id} {player.Shekels}");
            }

            if (msg != null && msg.Author.Id == this.Context.Client.CurrentUser.Id)
            {
                await msg.ModifyAsync(m => m.Content = stringBuilder.ToString());
            }
            else
            {
                await channel.SendMessageAsync(stringBuilder.ToString());
            }
        }

        public Task AddShekelsToPlayerAsync(Player player, int shekels)
        {
            player.AddShekels(shekels);
            this.AddOrUpdatePlayer(player);
            return this.UploadAsync();
        }

        private async Task<Player> GetPlayerAsync(ulong id)
        {
            var playersList = await this.DownloadAsync();
            return playersList.FirstOrDefault(p => p.Id == id) ?? new Player(id, 0);
        }

        /*private readonly List<Player> players = new List<Player>();

        public Player GetPlayer(ulong id)
        {
            Player p = null;
            foreach (var player in this.players)
            {
                if (player.Id == id)
                {
                    p = player;
                }
            }

            // check if null -> new Player(id, 0)
            p = p ?? new Player(id, 0);
            return p;
        }

        public string Export()
        {
            string ex = null;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var player in this.players)
            {
                ex = ex + player.Id + " " + player.Shekels + "\n";
            }

            return ex;
        }

        public async Task DownloadAsync(SocketCommandContext context)
        {
            // ReSharper disable once UsePatternMatching
            var channel = context.Guild.Channels.FirstOrDefault(ch => ch.Name.ToLowerInvariant() == "shekels") as SocketTextChannel;
            if (channel == null)
            {
                await context.Message.ReactAsync(EmojiExtensions.FromText("no_entry"));
                return;
            }

            var msgEnum = await channel.GetMessagesAsync(1).Flatten();
            var msg = msgEnum.FirstOrDefault();
            this.Import(msg);
        }

        private static Task UploadAsync(SocketCommandContext context, string output)
        {
            // ReSharper disable once UsePatternMatching
            var channel = context.Guild.Channels.FirstOrDefault(ch => ch.Name.ToLowerInvariant() == "shekels") as SocketTextChannel;
            ////var channel = context.Client.GetChannel(332508519179878400) as SocketTextChannel;
            return channel == null ? context.Message.ReactAsync(EmojiExtensions.FromText("no_entry")) : channel.SendMessageAsync(output);
        }

        private void AddOrUpdatePlayer(Player player)
        {
            var added = false;
            foreach (var p in this.players)
            {
                if (p.Id != player.Id)
                {
                    continue;
                }

                p.Shekels = player.Shekels;
                added = true;
                break;
            }

            if (added == false)
            {
                this.players.Add(player);
            }
        }

        private void Import(IMessage message)
        {
            var str = message.Content;
            var sr = new StringReader(str);
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                var split = s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                // string[] split = s.Split(new Char[] {';', ' '});
                var id = Convert.ToUInt64(split[0]);
                var shekels = Convert.ToInt32(split[1]);
                var p = new Player(id, shekels);
                this.AddOrUpdatePlayer(p);
            }
        }*/
    }
}
