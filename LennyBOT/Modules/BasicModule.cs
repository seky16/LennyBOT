using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOT.Config;
using LennyBOT.Services;
using System.Threading.Tasks;
using System;
using System.Linq;


namespace LennyBOT.Modules
{
    [Name("Basic")]
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        private readonly FactService _service;

        public BasicModule(FactService service)
        {
            _service = service;
        }

        [Command("nick")]
        [Remarks("Make the bot change your nick")]
        [MinPermissions(AccessLevel.User)]
        public async Task NickCmd([Remainder]string name)
        {
            var user = Context.User as SocketGuildUser;
            await user.ModifyAsync(x => x.Nickname = name);
            await ReplyAsync($"{user.Mention} I changed your name to **{name}**");
        }

        [Command("kevin")]
        //[Remarks("Make the bot say something")]
        [MinPermissions(AccessLevel.User)]
        public async Task KevCmd()
        {
            await Context.Channel.SendFileAsync("files/kevin.jpg");
        }

        [Command("fact")]
        [Remarks("Post random fact")]
        [MinPermissions(AccessLevel.User)]
        public async Task FactCmd()
        {
            string factToPost = await _service.GetFact();
            await ReplyAsync(factToPost);
        }

        [Command("userinfo"), Alias("user", "u", "whois")]
        [Remarks("Get info about user")]
        [MinPermissions(AccessLevel.User)]
        public async Task UserInfoCmd(SocketUser user = null)
        {
            user = user ?? Context.User;

            var g = user.Game.ToString();
            if (String.IsNullOrWhiteSpace(g)) { g = "*None*"; }

            var roles = (user as IGuildUser).RoleIds;
            string r = null;
            foreach (var role in roles)
            {
                var rol = Context.Guild.GetRole(role);
                r = r + rol.ToString() + "; ";
            }
            r = r.Remove(r.Length - 2);

            var nick = (user as IGuildUser)?.Nickname ?? user.Username;
            //if (String.IsNullOrWhiteSpace(nick)) { nick = user.Username; }

            await ReplyAsync($"```\n" +
                                $"username: {user.ToString()}\n" +
                                $"nickname: {nick}\n" +
                                $"      id: {user.Id}\n" +
                                $" created: {user.CreatedAt}\n" +
                                $"  joined: {(user as IGuildUser).JoinedAt}\n" +
                                $"  status: {user.Status}\n" +
                                $" playing: {g}\n" +
                                $"   roles: {r}\n" +
                                $"```");
            //\n avatar: {user.GetAvatarUrl()}");
        }

        [Command("quote", RunMode = RunMode.Async), Alias("q")]
        [Remarks("quote message")]
        [MinPermissions(AccessLevel.User)]
        public async Task QuoteCmd(ulong id)
        {
            IMessage msg = null;
            var bot = Context.Guild.CurrentUser;
            foreach (var ch in Context.Guild.TextChannels)
            {
                if (!(ch.Users.Contains(bot))) continue;
                msg = await ch.GetMessageAsync(id);
                if (msg != null) break;
            }
            //msg = await Context.Channel.GetMessageAsync(id);
            if (msg == null)
                await ReplyAsync($"Could not find message with id {id} in this guild.");
            else if (!(msg.Embeds.Count == 0))
                await ReplyAsync($"You cannot quote embed message.");
            else if (!(msg.Attachments.Count == 0))
                await ReplyAsync($"You cannot quote message with attachment.");
            else
            {
                var author = msg.Author as SocketGuildUser;
                string nick = author?.Nickname ?? msg.Author.Username;
                Color color = author.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Color;
                var guildIcon = author.Guild.IconUrl;
                Embed embed = new EmbedBuilder()
                    .WithDescription(msg.Content)
                    .WithColor(color)
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText($"#{msg.Channel}")
                            .WithIconUrl(guildIcon);
                    })
                    .WithTimestamp(msg.Timestamp)
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName(nick ?? "")
                        .WithIconUrl(author?.GetAvatarUrl() ?? ""))
                    .Build();
                await ReplyAsync("", false, embed);
            }
        }
    }
}