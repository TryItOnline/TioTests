using System;

namespace TioTests
{
    public static class Logger
    {
        private static string GetTime()
        {
            return DateTime.UtcNow.ToString("dd MMM hh:mm:ss");
        }
        public static void Log(string message)
        {
            Console.Write($"{GetTime()} message");
        }
        public static void LogLine(string message)
        {
            Console.WriteLine($"{GetTime()} message");
        }
    }
}
