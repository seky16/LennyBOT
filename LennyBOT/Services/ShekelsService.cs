// ReSharper disable StyleCop.SA1600

namespace LennyBOT.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public class ShekelsService
    {
        private readonly List<Player> players = new List<Player>();

        public static Task UploadAsync(SocketCommandContext context, string output)
        {
            var channel = context.Client.GetChannel(332508519179878400) as SocketTextChannel;
            return channel?.SendMessageAsync(output);
        }

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
            if (context.Client.GetChannel(332508519179878400) is SocketTextChannel channel)
            {
                var msgEnum = await channel.GetMessagesAsync(1).Flatten().ConfigureAwait(false);
                var msg = msgEnum.FirstOrDefault();
                this.Import(msg);
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
        }
    }
}
