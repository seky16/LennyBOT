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
}
