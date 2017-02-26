using System;

namespace TioTests
{
    public static class TimeFormatter
    {
        public static string FormatTime(TimeSpan interval)
        {
            if (interval < TimeSpan.FromSeconds(1))
            {
                return $"{FormatTime(interval.TotalMilliseconds)}ms";
            }
            if (interval < TimeSpan.FromMinutes(1))
            {
                return $"{FormatTime(interval.TotalSeconds)}s";
            }
            return $"{FormatTime(interval.TotalMinutes)}m{FormatTime(interval.Seconds)}s";
        }

        private static string FormatTime(double value)
        {
            var ceiling = (int)Math.Floor(value);
            return string.Format($"{ceiling:#,##0}");
        }
    }
}
