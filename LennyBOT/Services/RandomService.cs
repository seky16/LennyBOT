// ReSharper disable StyleCop.SA1600

namespace LennyBOT.Services
{
    using System;
    using System.Security.Cryptography;

    // http://stackoverflow.com/a/37804448
    public class RandomService
    {
        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public int Between(int min, int max)
        {
            // definuje array bytů
            var randomNumber = new byte[1];

            // "Fills an array of bytes with a cryptographically strong sequence of random values"
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rngcryptoserviceprovider(v=vs.110).aspx
            this.rng.GetBytes(randomNumber);

            // převede do typu double
            var rngD = Convert.ToDouble(randomNumber[0]);

            // vydělí 255 a tedy máme číslo mezi 0 a 1
            var multiplier = Math.Abs(rngD / 255d);

            // ze zadaných max a min spočítá rozsah, připočítá 1 pro zaokrouhlování
            var range = max - min + 1;

            // rozsah vynásobí koeficientem, zaokrouhlí dolů
            var randomValue = Math.Floor(multiplier * range);

            return (int)(min + randomValue);
        }
    }
}