using System;

namespace TioTests
{
    public static class TimeFormatter
    {
        private static readonly TimeSpan MinimumIntervalDeemedOneDay = TimeSpan.FromHours(23);
        private static readonly TimeSpan MinimumIntervalDeemedOneHour = TimeSpan.FromMinutes(59);
        private static readonly TimeSpan MinimumIntervalDeemedOneMinute = TimeSpan.FromSeconds(59);
        private static readonly TimeSpan MinimumIntervalDeemedOneSecond = TimeSpan.FromMilliseconds(999);

        public static string LargestIntervalWithUnits(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval",
                                                    "Negative Timespans are not supported.");
            }

            if (interval > MinimumIntervalDeemedOneDay)
            {
                return FormatTime(interval.TotalDays, "day");
            }

            if (interval > MinimumIntervalDeemedOneHour)
            {
                return FormatTime(interval.TotalHours, "hour");
            }

            if (interval > MinimumIntervalDeemedOneMinute)
            {
                return FormatTime(interval.TotalMinutes, "minute");
            }

            if (interval > MinimumIntervalDeemedOneSecond)
            {
                return FormatTime(interval.TotalSeconds, "second");
            }

            return "now";
        }

        private static string FormatTime(double value, string units)
        {
            var ceiling = (int)Math.Ceiling(value);
            return string.Format("{0:#,##0} {1}{2}",
                                ceiling,
                                units,
                                (ceiling == 1 ? string.Empty : "s"));
        }
    }
}