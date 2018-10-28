// ReSharper disable StyleCop.SA1600
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
namespace LennyBOT.Models
{
    using System;

    public class Tag
    {
        public Tag(string name, string content, ulong ownerId)
        {
            this.Name = name;
            this.Content = content;
            this.OwnerId = ownerId;
            this.CreatedAt = DateTimeOffset.UtcNow;
        }

        public string Name { get; set; }

        public string Content { get; set; }

        public ulong OwnerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}