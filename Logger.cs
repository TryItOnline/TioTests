using System;

namespace TioTests
{
    public static class Logger
    {
        private static bool _timeFlag = true;
        private static string GetTime()
        {
            return DateTime.UtcNow.ToString("dd MMM hh:mm:ss UTC");
        }
        public static void Log(string message, bool setTime = false)
        {
            Console.Write(_timeFlag ? $"{GetTime()} {message}" : $"{message}");
            _timeFlag = setTime;
        }
        public static void LogLine(string message)
        {
            Console.WriteLine(_timeFlag ? $"{GetTime()} {message}" : $"{message}");
            _timeFlag = true;
        }
    }
}
