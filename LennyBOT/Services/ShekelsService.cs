using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LennyBOT.Services
{
    public class ShekelsService
    {
        List<Player> players = new List<Player>();

        public void AddOrUpdatePlayer(Player player)
        {
            bool added = false;
            foreach (var p in players)
            {
                if (p.Id == player.Id) { p.Shekels = player.Shekels; added = true; break; }
                else continue;
            }
            if (added == false) { players.Add(player); }
        }

        public Player GetPlayer(ulong id)
        {
            Player p = null;
            foreach (var player in players)
            {
                if (player.Id == id) { p = player; }
                else continue;
            }
            // check if null -> new Player(id, 0)
            p = p ?? new Player(id, 0);
            return p;
        }

        public string Export()
        {
            string ex = null;
            foreach (var player in players)
            {
                ex = ex + player.Id + " " + player.Shekels + "\n";
            }
            return ex;
        }

        public void Import(IMessage message)
        {
            string str = message.Content;
            var sr = new StringReader(str);
            string s = String.Empty;
            while ((s = sr.ReadLine()) != null)
            {
                char[] separator = null;
                string[] split = s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                //string[] split = s.Split(new Char[] {';', ' '});
                ulong id = Convert.ToUInt64(split[0]);
                int shekels = Convert.ToInt32(split[1]);
                var p = new Player(id, shekels);
                AddOrUpdatePlayer(p);
            }
        }

        public async Task Download(SocketCommandContext context)
        {
            var channel = context.Client.GetChannel(332508519179878400) as SocketTextChannel;
            var msgEnum = await channel.GetMessagesAsync(1).Flatten();
            var msg = msgEnum.FirstOrDefault();
            Import(msg);
        }

        public async Task Upload(SocketCommandContext context, string output)
        {
            var channel = context.Client.GetChannel(332508519179878400) as SocketTextChannel;
            await channel.SendMessageAsync(output);
        }
    }

    public class Player
    {
        public ulong Id { get; set; }
        public int Shekels { get; set; }

        public Player(ulong id, int shekels)
        {
            Id = id;
            Shekels = shekels;
        }
        public Player(IUser user, int shekels)
        {
            Id = user.Id;
            Shekels = shekels;
        }

        public void AddShekels(int amount)
        {
            Shekels = Shekels + amount;
        }

        public void RemoveShekels(int amount)
        {
            Shekels = Shekels - amount;
        }
    }
}
