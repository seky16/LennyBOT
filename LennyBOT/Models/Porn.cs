﻿// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class Porn
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("result")]
        public List<VideoModel> VideoModel { get; set; }
    }

    // ReSharper disable once StyleCop.SA1402
    public class VideoModel
    {
        [JsonProperty("url")]
        public string VideoUrl { get; set; }
        [JsonProperty("thumb")]
        public string VideoThumb { get; set; }
        [JsonProperty("title")]
        public string VideoTitle { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("views")]
        public int Views { get; set; }
    }
}