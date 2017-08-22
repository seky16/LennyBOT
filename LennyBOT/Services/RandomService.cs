using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LennyBOT.Services
{
    // http://stackoverflow.com/a/37804448
    public class RandomService
    {
        private readonly System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();

        public int Between(int min, int max)
        {
            // definuje array bytů
            byte[] randomNumber = new byte[1];

            // "Fills an array of bytes with a cryptographically strong sequence of random values"
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rngcryptoserviceprovider(v=vs.110).aspx
            rng.GetBytes(randomNumber);

            // převede do typu double
            double rng_d = Convert.ToDouble(randomNumber[0]);

            // vydělí 255 a tedy máme číslo mezi 0 a 1
            double multiplier = Math.Abs(rng_d / 255d);

            // ze zadaných max a min spočítá rozsah, připočítá 1 pro zaokrouhlování
            int range = max - min + 1;

            // rozsah vynásobí koeficientem, zaokrouhlí dolů
            double randomValue = Math.Floor(multiplier * range);

            return (int)(min + randomValue);
        }
    }
}