// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Config
{
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;

    public class LennyBase : ModuleBase<SocketCommandContext>
    {
        protected static readonly Emoji Pass = EmojiExtensions.FromText("ok_hand");
        protected static readonly Emoji Fail = EmojiExtensions.FromText("no_entry");
        protected static readonly Emoji Question = EmojiExtensions.FromText("question");
        protected static readonly Emoji Removed = EmojiExtensions.FromText("put_litter_in_its_place");

        protected static string GetNickname(IUser user)
        {
            var guildUser = user as IGuildUser;
            return guildUser?.Nickname ?? user.Username;
        }

        protected Task ReactAsync(IEmote emoji)
            => this.Context.Message.AddReactionAsync(emoji);
    }
}