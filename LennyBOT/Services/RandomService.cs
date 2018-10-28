// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System;
    using System.Security.Cryptography;

    public class RandomService
    {
        /// <summary>
        /// Generate true random number (http://stackoverflow.com/a/37804448)
        /// </summary>
        /// <param name="min">
        /// Minimal value to generate (inclusive).
        /// </param>
        /// <param name="max">
        /// Maximal value to generate (inclusive).
        /// </param>
        /// <returns>
        /// <see cref="int"/>
        /// </returns>
        public static int Generate(int min, int max)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                // definuje array bytů
                var randomNumber = new byte[1];

                // "Fills an array of bytes with a cryptographically strong sequence of random values"
                // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rngcryptoserviceprovider(v=vs.110).aspx
                rng.GetBytes(randomNumber);

                // převede do typu double
                var rngD = Convert.ToDouble(randomNumber[0]);

                // vydělí 255 a tedy máme číslo mezi 0 a 1
                var multiplier = Math.Max(0, (rngD / 255d) - 0.00000000001d);

                // ze zadaných max a min spočítá rozsah, připočítá 1 pro zaokrouhlování
                var range = max - min + 1;

                // rozsah vynásobí koeficientem, zaokrouhlí dolů
                var randomValue = Math.Floor(multiplier * range);

                return (int)(min + randomValue);
            }
        }
    }
}