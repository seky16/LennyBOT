// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Services;

    [Name("Basic")]
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        private readonly FactService service;

        public BasicModule(FactService service)
        {
            this.service = service;
        }

        [Command("nick")]
        [Remarks("Make the bot change your nick")]
        [MinPermissions(AccessLevel.User)]
        public async Task NickCmdAsync([Remainder]string name)
        {
            if (this.Context.User is SocketGuildUser user)
            {
                await user.ModifyAsync(x => x.Nickname = name).ConfigureAwait(false);
                await this.ReplyAsync($"{user.Mention} I changed your name to **{name}**").ConfigureAwait(false);
            }
        }

        [Command("kevin")]
        [MinPermissions(AccessLevel.User)]
        public Task KevCmdAsync()
        {
            return this.Context.Channel.SendFileAsync("files/kevin.jpg");
        }

        [Command("fact")]
        [Remarks("Post random fact")]
        [MinPermissions(AccessLevel.User)]
        public async Task FactCmdAsync()
        {
            var factToPost = await this.service.GetFactAsync().ConfigureAwait(false);
            await this.ReplyAsync(factToPost).ConfigureAwait(false);
        }

        [Command("userinfo"), Alias("user", "u", "whois")]
        [Remarks("Get info about user")]
        [MinPermissions(AccessLevel.User)]
        public Task UserInfoCmdAsync(SocketUser user = null)
        {
            user = user ?? this.Context.User;

            var g = user.Game.ToString();
            if (string.IsNullOrWhiteSpace(g))
            {
                g = "*None*";
            }

            var roles = (user as IGuildUser)?.RoleIds;
            string r = null;
            // ReSharper disable once LoopCanBeConvertedToQuery
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    var rol = this.Context.Guild.GetRole(role);
                    r = r + rol + "; ";
                }
            }

            r = r?.Remove(r.Length - 2);

            var nick = (user as IGuildUser)?.Nickname ?? user.Username;

            // if (String.IsNullOrWhiteSpace(nick)) { nick = user.Username; }
            return this.ReplyAsync($"```\nusername: {user}\nnickname: {nick}\n      id: {user.Id}\n created: {user.CreatedAt}\n  joined: {(user as IGuildUser)?.JoinedAt}\n  status: {user.Status}\n playing: {g}\n   roles: {r}\n```");

            // \n avatar: {user.GetAvatarUrl()}");
        }

        [Command("quote", RunMode = RunMode.Async), Alias("q")]
        [Remarks("quote message")]
        [MinPermissions(AccessLevel.User)]
        public async Task QuoteCmdAsync(ulong id)
        {
            IMessage msg = null;
            var bot = this.Context.Guild.CurrentUser;
            foreach (var ch in this.Context.Guild.TextChannels)
            {
                if (!ch.Users.Contains(bot))
                {
                    continue;
                }

                msg = await ch.GetMessageAsync(id).ConfigureAwait(false);
                if (msg != null)
                {
                    break;
                }
            }

            // msg = await Context.Channel.GetMessageAsync(id);
            if (msg == null)
            {
                await this.ReplyAsync($"Could not find message with id {id} in this guild.").ConfigureAwait(false);
            }
            else if (msg.Embeds.Count != 0)
            {
                await this.ReplyAsync("You cannot quote embed message. Coming soon™.").ConfigureAwait(false);
            }
            else if (msg.Attachments.Count != 0)
            {
                await this.ReplyAsync("You cannot quote message with attachment. Coming soon™.").ConfigureAwait(false);
            }
            else
            {
                var author = msg.Author as SocketGuildUser;
                var nick = author?.Nickname ?? msg.Author.Username;
                var color = author?.Roles.OrderByDescending(x => x.Position).FirstOrDefault()?.Color ?? Color.Default;
                var guildIcon = author?.Guild.IconUrl;
                var embed = new EmbedBuilder().WithDescription(msg.Content).WithColor(color)
                    .WithFooter(footer => { footer.WithText($"#{msg.Channel}").WithIconUrl(guildIcon); })
                    .WithTimestamp(msg.Timestamp).WithAuthor(
                        new EmbedAuthorBuilder().WithName(nick ?? string.Empty)
                            .WithIconUrl(author?.GetAvatarUrl() ?? string.Empty)).Build();
                await this.ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
        }
    }
}