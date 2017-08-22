using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace LennyBOT.Services
{
    public class FactService
    {
        List<string> randomFacts;
        int numOfFacts = 0;

        public FactService()
        {
            randomFacts = new List<string>();
            StreamReader sr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "/files/randFacts.txt");
            string s = String.Empty;
            while ((s = sr.ReadLine()) != null)
            {
                randomFacts.Add(s);
                numOfFacts++;
            }
        }

        public async Task<string> GetFact()
        {
            var rand = new RandomService();
            int randomFactIndex = rand.Between(0, numOfFacts - 1);
            string factToPost = randomFacts[randomFactIndex];
            await Task.Delay(0);
            return factToPost;
        }
    }
}