// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System.IO;
    using System.Linq;

    using LennyBOT.Models;

    using Newtonsoft.Json;

    public class TagService
    {
    public static void CreateTag(string name, string content, ulong ownerId)
        {
            var createdTag = new Tag(name, content, ownerId);
            var jsonString = File.ReadAllText("Files\\tags.json");
            var arr = JsonConvert.DeserializeObject<Tag[]>(jsonString);
            var list = arr.ToList();
            list.Add(createdTag);
            arr = list.ToArray();
            jsonString = JsonConvert.SerializeObject(arr);
            File.WriteAllText("Files\\tags.json", jsonString);
        }

        public static Tag GetTag(string name)
        {
            var jsonString = File.ReadAllText("Files\\tags.json");
            var arr = JsonConvert.DeserializeObject<Tag[]>(jsonString);
            var tag = arr.FirstOrDefault(t => t.Name == name);
            return tag;
        }

        public static bool RemoveTag(Tag tagToRemove)
        {
            var jsonString = File.ReadAllText("Files\\tags.json");
            var arr = JsonConvert.DeserializeObject<Tag[]>(jsonString);
            var list = arr.ToList();
            var success = list.Remove(list.Find(t => t.Name == tagToRemove.Name));
            if (!success)
            {
                return false;
            }

            arr = list.ToArray();
            jsonString = JsonConvert.SerializeObject(arr);
            File.WriteAllText("Files\\tags.json", jsonString);
            return true;
        }

        public static string GetListOfTags()
        {
            var jsonString = File.ReadAllText("Files\\tags.json");
            var arr = JsonConvert.DeserializeObject<Tag[]>(jsonString);
            var list = arr.ToList();
            var names = list.Select(tag => tag.Name).ToList();
            var output = string.Join(", ", names);
            return output;
        }
    }
}