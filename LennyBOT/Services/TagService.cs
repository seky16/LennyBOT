// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using Discord;

    using LiteDB;

    public class TagService
    {
        public TagService(LiteDatabase database)
        {
            this.Database = database;
        }

        private LiteDatabase Database { get; set; }

        public void CreateTag(string name, string content, IUser owner)
        {
            var createdTag = new Tag(name, content, owner);
            var tags = this.Database.GetCollection<Tag>("tags");
            tags.Insert(createdTag);
        }

        public Tag GetTag(string name)
        {
            var tags = this.Database.GetCollection<Tag>("tags");
            return tags.FindOne(t => t.Name == name);
        }
    }
}