using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanDictionnet;

namespace LennyBOT.Services
{
    public class SearchService
    {
        public UrbanClient UrbanClient { get; set; }

        public SearchService()
        {
            UrbanClient = new UrbanClient();
        }

        public string Extract(string input, string start, string end)
        {
            int startNum = 0, endNum = 0;
            startNum = input.IndexOf(start) + start.Length;
            input = input.Remove(0, startNum);
            endNum = input.IndexOf(end);
            input = input.Remove(endNum);
            return input;
        }
    }

    public class Inventory
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
    }
}
