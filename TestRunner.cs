using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace TioTests
{
    public class TestRunner
    {
        public static void RunTest(string file, Config config)
        {
            var name = file.EndsWith(".json") ? file.Substring(0, file.Length - ".json".Length) : file;
            name = Path.GetFileName(name);
            Logger.Log(config.UseConsoleCodes ? $"{name}..." : $"{name} ");
            TestDescription test = JsonConvert.DeserializeObject<TestDescription>(Encoding.UTF8.GetString(File.ReadAllBytes(file)));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            RunResult result = Execute(test.Input, config.RunUrl);
            sw.Stop();
            string time = TimeFormatter.LargestIntervalWithUnits(sw.Elapsed);

            if (config.TrimWhitespacesFromResults)
            {
                result.Output = result.Output?.Trim("\n\r\t ".ToCharArray());
            }
            if (config.UseConsoleCodes)
            {
                Logger.Log($"\r{string.Empty.PadLeft(name.Length + 23)}\r", true);
            }
            if (test.Output == result.Output)
            {
                if (config.UseConsoleCodes) Console.ForegroundColor = ConsoleColor.Green;
                Logger.LogLine(config.UseConsoleCodes ? $"{name} - PASS ({time})" : $"- PASS ({time})");
                if (config.UseConsoleCodes)  Console.ResetColor();
                if (config.DisplayDebugInfoOnSuccess)
                {
                    if (result.Warnings != null)
                    {
                        foreach (string warning in result.Warnings)
                        {
                            Logger.LogLine($"Warning: {warning}");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(result.Debug))
                    {
                        Logger.LogLine($"Debug {result.Debug}");
                    }
                }
            }
            else
            {
                if (config.UseConsoleCodes)  Console.ForegroundColor = ConsoleColor.Red;
                Logger.LogLine(config.UseConsoleCodes ? $"{name} - FAIL ({time})" : $"- FAIL ({time})");
                if (config.UseConsoleCodes)  Console.ResetColor();
                if (config.DisplayDebugInfoOnError)
                {
                    Logger.LogLine($"Expected: {test.Output}");
                    Logger.LogLine($"Got: {result.Output}");
                    if (result.Warnings != null)
                    {
                        foreach (string warning in result.Warnings)
                        {
                            Logger.LogLine($"Warning: {warning}");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(result.Debug))
                    {
                        Logger.LogLine($"Debug {result.Debug}");
                    }
                }
            }
        }

        private static RunResult Execute(Runnable test, string configRunUrl)
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri(configRunUrl) };
            HttpContent z = new ByteArrayContent(test.GetBytes());
            HttpResponseMessage response = client.PostAsync(configRunUrl, z).Result;
            string s = response.Content.ReadAsStringAsync().Result;
            if (s.Length < 16)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Server error. Server returned [{s}]" }
                };
            }
            string separator = s.Substring(0, 16);
            s = s.Substring(16);
            string[] tokens = s.Split(new[] { separator }, StringSplitOptions.None);
            if (tokens.Length != 3)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't parse result [{s}]" }
                };
            }
            return new RunResult
            {
                Warnings = tokens[2]?.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                Output = tokens[0],
                Debug = tokens[1]
            };
        }
    }
}
