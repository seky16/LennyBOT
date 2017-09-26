// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Services;

    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Emoji TagNotFound = EmojiExtensions.FromText("question");
        private static readonly Emoji Pass = EmojiExtensions.FromText("ok_hand");
        private static readonly Emoji Fail = EmojiExtensions.FromText("no_entry");
        private static readonly Emoji Removed = EmojiExtensions.FromText("put_litter_in_its_place");

        private static readonly List<string> Reserved = new List<string> { "create", "remove", "delete", "info", "owner" };

        [Command]
        [Remarks("List available tags")]
        [MinPermissions(AccessLevel.User)]
        public Task TagAsync()
        {
            return this.ReplyAsync(TagService.GetListOfTags());
        }

        [Command]
        [Remarks("Show a saved tag")]
        [MinPermissions(AccessLevel.User)]
        public Task TagAsync([Remainder] string name)
        {
            var tag = TagService.GetTag(name);
            return tag == null ? this.ReactAsync(TagNotFound) : this.ReplyAsync($"**{tag.Name}:** {tag.Content}");
        }

        [Command("create"), Priority(100)]
        [Remarks("Create new tag")]
        [MinPermissions(AccessLevel.User)]
        public Task CreateTagAsync(string name, [Remainder] string content)
        {
            if (TagService.GetTag(name) != null)
            {
                return this.ReactAsync(Fail);
            }

            if (Reserved.Contains(name))
            {
                return this.ReactAsync(Fail);
            }

            TagService.CreateTag(name, content, this.Context.User.Id);
            return this.ReactAsync(Pass);
        }

        [Command("remove"), Priority(99)]
        [Alias("delete")]
        [Remarks("Remove tag (**yours only**)")]
        [MinPermissions(AccessLevel.User)]
        public Task RemoveTagAsync(string name)
        {
            var tag = TagService.GetTag(name);
            if (tag == null)
            {
                return this.ReactAsync(Fail);
            }

            if (tag.OwnerId != this.Context.User.Id || !Configuration.Load().Owners.Contains(this.Context.User.Id))
            {
                return this.ReactAsync(Fail);
            }

            var success = TagService.RemoveTag(tag);
            return this.ReactAsync(success ? Removed : Fail);
        }

        [Command("info"), Priority(98)]
        [Alias("owner")]
        [Remarks("Get info about tag (name, owner, time of creation")]
        [MinPermissions(AccessLevel.User)]
        public Task TagInfoAsync(string name)
        {
            var tag = TagService.GetTag(name);
            if (tag == null)
            {
                return this.ReactAsync(Fail);
            }

            var nick = string.Empty;
            foreach (var user in this.Context.Guild.Users)
            {
                if (tag.OwnerId != user.Id)
                {
                    continue;
                }

                nick = user.Nickname + " ";
                break;
            }

            var output = "```\n"
                + $"      Name: {tag.Name}\n"
                + $"     Owner: {nick}({this.Context.Client.GetUser(tag.OwnerId)})\n"
                + $"Created at: {tag.CreatedAt}\n"
                + "```";
            return this.ReplyAsync(output);
        }

        private Task ReactAsync(IEmote emoji)
            => this.Context.Message.AddReactionAsync(emoji);
    }
}
