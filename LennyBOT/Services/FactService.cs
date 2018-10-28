// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class FactService
    {
        private readonly List<string> randomFacts;

        private readonly int numOfFacts;

        public FactService()
        {
            this.randomFacts = new List<string>();
            var sr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "Files/randFacts.txt");
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                this.randomFacts.Add(s);
                this.numOfFacts++;
            }
        }

        public async Task<string> GetFactAsync()
        {
            var randomFactIndex = RandomService.Generate(0, this.numOfFacts - 1);
            var factToPost = this.randomFacts[randomFactIndex];
            await Task.Delay(0);
            return factToPost;
        }
    }
}