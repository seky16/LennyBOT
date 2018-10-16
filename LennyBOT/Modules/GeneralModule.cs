// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Cookie.FOAAS;

    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Services;

    [Name("General")]
    public class GeneralModule : LennyBase
    {
        private readonly FactService service;

        public GeneralModule(FactService service)
        {
            this.service = service;
        }

        [Command("nick")]
        [Remarks("Make the bot change your nick")]
        [MinPermissions(AccessLevel.User)]
        public async Task NickCmdAsync([Remainder] string name)
        {
            if (this.Context.User is SocketGuildUser user)
            {
                await user.ModifyAsync(x => x.Nickname = name).ConfigureAwait(false);
                await this.ReplyAsync($"{user.Mention} I changed your name to **{name}**").ConfigureAwait(false);
            }
        }

        [Command("kevin")]
        [Remarks("Fuck Kevin")]
        [MinPermissions(AccessLevel.User)]
        public Task KevCmdAsync()
        {
            return this.Context.Channel.SendFileAsync("Files/kevin.jpg");
        }

        [Command("fact")]
        [Remarks("Post random fact")]
        [MinPermissions(AccessLevel.User)]
        public async Task FactCmdAsync()
        {
            var factToPost = await this.service.GetFactAsync().ConfigureAwait(false);
            await this.ReplyAsync(factToPost).ConfigureAwait(false);
        }

        [Command("quote"), Alias("q")]
        [Remarks("Quote message by id")]
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

            var attachmentUrl = string.Empty;
            if (msg == null)
            {
                await this.ReplyAsync($"Could not find message with id {id} in this guild.").ConfigureAwait(false);
                return;
            }

            if (msg.Embeds.Count != 0)
            {
                var embed = msg.Embeds.FirstOrDefault() as Embed;
                await this.ReplyAsync(msg.Content, false, embed).ConfigureAwait(false);
                return;
            }

            if (msg.Attachments.Count != 0)
            {
                attachmentUrl = msg.Attachments.FirstOrDefault()?.Url;
            }

            var author = msg.Author as SocketGuildUser;
            var nick = author?.Nickname ?? msg.Author.Username;
            var color = author?.Roles.OrderByDescending(x => x.Position).FirstOrDefault()?.Color ?? Color.Default;
            var guildIcon = author?.Guild.IconUrl ?? string.Empty;
            var builder = new EmbedBuilder().WithDescription(msg.Content).WithColor(color)
                .WithFooter(footer => { footer.WithText($"#{msg.Channel}").WithIconUrl(guildIcon); })
                .WithTimestamp(msg.EditedTimestamp ?? msg.Timestamp).WithImageUrl(attachmentUrl).WithAuthor(
                    new EmbedAuthorBuilder().WithName(nick ?? string.Empty)
                        .WithIconUrl(author?.GetAvatarUrl() ?? string.Empty));
            await this.ReplyAsync(string.Empty, false, builder.Build()).ConfigureAwait(false);
        }

        [Command("quote"), Alias("q")]
        [Remarks("Quote custom message")]
        [MinPermissions(AccessLevel.User)]
        public Task QuoteCmdAsync(IUser user, [Remainder] string content)
        {
            var authorNick = GetNickname(this.Context.Message.Author);
            var guildUser = user as SocketGuildUser;
            var nick = guildUser?.Nickname ?? user.Username;
            var color = guildUser?.Roles.OrderByDescending(x => x.Position).FirstOrDefault()?.Color ?? Color.Default;
            var guildIcon = guildUser?.Guild.IconUrl ?? string.Empty;
            var builder = new EmbedBuilder().WithDescription(content).WithColor(color)
                .WithFooter(footer => { footer.WithText($"quoted by {authorNick}").WithIconUrl(guildIcon); })
                .WithCurrentTimestamp().WithAuthor(
                    new EmbedAuthorBuilder().WithName(nick ?? string.Empty)
                        .WithIconUrl(guildUser?.GetAvatarUrl() ?? string.Empty));
            return this.ReplyAsync(string.Empty, false, builder.Build());
        }

        [Command("emojify"), Alias("emoji")]
        [MinPermissions(AccessLevel.User)]
        public Task EmojifyAsync([Remainder] string text)
        {
            var stringBuilder = new StringBuilder();
            foreach (var ch in text.ToLowerInvariant())
            {
                switch (ch)
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        stringBuilder.Append($":regional_indicator_{ch}: ");
                        break;
                    case '0':
                        stringBuilder.Append(":zero: ");
                        break;
                    case '1':
                        stringBuilder.Append(":one: ");
                        break;
                    case '2':
                        stringBuilder.Append(":two: ");
                        break;
                    case '3':
                        stringBuilder.Append(":three: ");
                        break;
                    case '4':
                        stringBuilder.Append(":four: ");
                        break;
                    case '5':
                        stringBuilder.Append(":five: ");
                        break;
                    case '6':
                        stringBuilder.Append(":six: ");
                        break;
                    case '7':
                        stringBuilder.Append(":seven: ");
                        break;
                    case '8':
                        stringBuilder.Append(":eight: ");
                        break;
                    case '9':
                        stringBuilder.Append(":nine: ");
                        break;
                    case '!':
                        stringBuilder.Append(":exclamation: ");
                        break;
                    case '?':
                        stringBuilder.Append(":question: ");
                        break;
                    case '+':
                        stringBuilder.Append(":heavy_plus_sign: ");
                        break;
                    case '-':
                        stringBuilder.Append(":heavy_minus_sign: ");
                        break;
                    /*case '×':
                        stringBuilder.Append(":heavy_multiplication_x: ");
                        break;
                    case '÷':
                        stringBuilder.Append(":heavy_division_sign: ");
                        break;*/
                    case '$':
                        stringBuilder.Append(":heavy_dollar_sign: ");
                        break;
                    default:
                        stringBuilder.Append($"**{ch.ToString().ToUpper()}** ");
                        break;
                }
            }

            return this.ReplyAsync(stringBuilder.ToString());

            /*[Command("fuck"), Remarks("Gives a random fuck about your useless fucking command.")]
            public async Task FoaasAsync(IGuildUser user = null)
                => await this.ReplyAsync(await FOAAS.RandomAsync(From: this.Context.User.Username, Name: user != null ? GetNickname(user) : "Kevin").ConfigureAwait(false)).ConfigureAwait(false);
            */
        }
    }
}
