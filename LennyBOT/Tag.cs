// ReSharper disable StyleCop.SA1600
namespace LennyBOT
{
    using System;

    using Discord;

    public class Tag
    {
        public Tag()
        {
        }

        public Tag(string name, string content, IUser owner)
        {
            this.Name = name;
            this.Content = content;
            this.Owner = owner;
            this.CreatedAt = DateTimeOffset.UtcNow;
        }

        public string Name { get; }

        public string Content { get; }

        public IUser Owner { get; }

        public DateTimeOffset CreatedAt { get; }

        public Embed Embed
        {
            get
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(this.Owner);
                builder.WithTitle(this.Name);
                builder.WithDescription(this.Content);
                builder.WithTimestamp(this.CreatedAt);
                return builder.Build();
            }
        }
    }
}
