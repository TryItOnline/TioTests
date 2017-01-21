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
            if (interval > MinimumIntervalDeemedOneDay)
            {
                return FormatTime(interval.TotalDays, "d");
            }

            if (interval > MinimumIntervalDeemedOneHour)
            {
                return FormatTime(interval.TotalHours, "hr");
            }

            if (interval > MinimumIntervalDeemedOneMinute)
            {
                return FormatTime(interval.TotalMinutes, "min");
            }

            if (interval > MinimumIntervalDeemedOneSecond)
            {
                return FormatTime(interval.TotalSeconds, "sec");
            }

            return FormatTime(interval.TotalMilliseconds, "ms");
        }

        private static string FormatTime(double value, string units)
        {
            var ceiling = (int)Math.Ceiling(value);
            return string.Format($"{ceiling:#,##0} {units}");
        }
    }
}