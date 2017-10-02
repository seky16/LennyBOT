// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System;

    using NodaTime;

    public class SearchService
    {
        public static string Extract(string input, string start, string end)
        {
            int startNum, endNum;
            startNum = input.IndexOf(start, StringComparison.InvariantCultureIgnoreCase) + start.Length;
            input = input.Remove(0, startNum);
            endNum = input.IndexOf(end, StringComparison.InvariantCultureIgnoreCase);
            input = input.Remove(endNum);
            return input;
        }

        public static string DateFromSeconds(long? seconds)
        {
            if (seconds == null)
            {
                return null;
            }

            var instant = Instant.FromUnixTimeSeconds((long)seconds);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour}:{date.Minute:D2}:{date.Second:D2}";
        }
    }
}
