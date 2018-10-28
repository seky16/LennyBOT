// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class List
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }
    }

    // ReSharper disable once StyleCop.SA1402
    public class Urban
    {
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("list")]
        public List<List> List { get; set; }
    }
}