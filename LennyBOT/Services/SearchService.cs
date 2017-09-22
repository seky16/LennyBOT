// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System;

    using UrbanDictionnet;

    public class SearchService
    {
        public SearchService()
        {
            this.UrbanClient = new UrbanClient();
        }

        public UrbanClient UrbanClient { get; }

        public static string Extract(string input, string start, string end)
        {
            int startNum, endNum;
            startNum = input.IndexOf(start, StringComparison.InvariantCultureIgnoreCase) + start.Length;
            input = input.Remove(0, startNum);
            endNum = input.IndexOf(end, StringComparison.InvariantCultureIgnoreCase);
            input = input.Remove(endNum);
            return input;
        }
    }

    /*public class Inventory
    {
        public bool Success { get; set; }
        public string Value { get; set; }
        public string Items { get; set; }
        public string Currency { get; set; }
    }

    public class OsuData
    {
        public string User_id { get; set; }
        public string Username { get; set; }
        public string Count300 { get; set; }
        public string Count100 { get; set; }
        public string Count50 { get; set; }
        public string Playcount { get; set; }
        public string Ranked_score { get; set; }
        public string Total_score { get; set; }
        public string Pp_rank { get; set; }
        public string Level { get; set; }
        public string Pp_raw { get; set; }
        public string Accuracy { get; set; }
        public string Count_rank_ss { get; set; }
        public string Count_rank_s { get; set; }
        public string Count_rank_a { get; set; }
        public string Country { get; set; }
        public string Pp_country_rank { get; set; }
        public object Events { get; set; }
    }*/
}
