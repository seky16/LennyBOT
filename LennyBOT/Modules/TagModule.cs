// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;

    using LennyBOT.Services;

    using LiteDB;

    public class TagModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Emoji TagNotFound = EmojiExtensions.FromText("mag_right");
        private static readonly Emoji Pass = EmojiExtensions.FromText("ok_hand");
        private static readonly Emoji Fail = EmojiExtensions.FromText("octagonal_sign");
        private static readonly Emoji Removed = EmojiExtensions.FromText("put_litter_in_its_place");

        protected TagModule(TagService service)
        {
            this.Service = service;
        }

        private TagService Service { get; set; }

        [Command("tag")]
        public Task TagAsync([Remainder] string name)
        {
            var tag = this.Service.GetTag(name);
            return tag == null ? this.ReplyAsync("Tag not found!") : this.ReplyAsync(string.Empty, false, tag.Embed);
        }

        [Command("tag create")]
        public Task CreateTagAsync(string name, [Remainder] string content)
        {
            this.Service.CreateTag(name, content, this.Context.User);
            return this.ReactAsync(Pass);
        }


        private Task ReactAsync(IEmote emoji)
            => this.Context.Message.AddReactionAsync(emoji);
    }
}
