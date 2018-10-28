// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Extensions
{
    using System;

    using NodaTime;

    public static class DateTimeOffsetExtension
    {
        /// <summary>
        /// Returns DateTimeOffset as time in Prague (using NodaTime) in custom format (dd. MM. yyyy hh:mm:ss).
        /// </summary>
        /// <param name="dateTimeOffset">
        /// The date time offset.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToPragueTimeString(this DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset == null)
            {
                return "???";
            }

            // ReSharper disable once ConstantNullCoalescingCondition
            var dateTimeOffsetNotNull = dateTimeOffset ?? new DateTimeOffset();
            var utcDateTime = dateTimeOffsetNotNull.UtcDateTime;
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour}:{date.Minute:D2}:{date.Second:D2}";
        }

        /// <summary>
        /// Returns DateTimeOffset as time in Prague (using NodaTime) in custom format (dd. MM. yyyy hh:mm:ss).
        /// </summary>
        /// <param name="dateTimeOffset">
        /// The date time offset.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToPragueTimeString(this DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = dateTimeOffset.UtcDateTime;
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour}:{date.Minute:D2}:{date.Second:D2}";
        }
    }
}