using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace TioTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            if (Directory.Exists(config.Test))
            {
                foreach (string file in Directory.GetFiles(config.Test,"*.json"))
                {
                    RunTest(file, config);
                }
            }
            else if (File.Exists(config.Test))
            {
                RunTest(config.Test, config);
            }
            else if (File.Exists(config.Test + ".json"))
            {
                RunTest(config.Test + ".json", config);
            }
            else
            {
                Console.WriteLine($"{config.Test} not found");
            }
        }

        private static void RunTest(string file, Config config)
        {
            var name = file.EndsWith(".json") ? file.Substring(0, file.Length - ".json".Length) : file;
            name = Path.GetFileName(name);
	    Console.Write($"{name}...");
            TestDescription test = JsonConvert.DeserializeObject<TestDescription>(File.ReadAllText(file));
            RunResult result = Execute(test.Input, config.RunUrl);
            if (config.TrimResults)
            {
                result.Output = result.Output.Trim("\n\r\t ".ToCharArray());
            }
	    Console.Write($"\r{string.Empty.PadLeft(name.Length+3)}\r");
            if (test.Output == result.Output)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{name} - PASS");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{name} - FAIL");
                Console.ResetColor();
                Console.WriteLine($"Expected: {test.Output}");
                Console.WriteLine($"Got: {result.Output}");
                if (result.Warnings != null)
                {
                    foreach (string warning in result.Warnings)
                    {
                        if (!string.IsNullOrWhiteSpace(warning))
                        {
                            Console.WriteLine($"Warning: {warning}");
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(result.Debug))
                {
                    Console.WriteLine($"Debug {result.Debug}");
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
                    Warnings = new List<string> {$"Server error. Server returned [{s}]"}
                };
            }
            string separator = s.Substring(0, 16);
            s = s.Substring(16);
            string[] tokens = s.Split(new [] { separator}, StringSplitOptions.None);
            if (tokens.Length != 3)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't parse result [{s}]" }
                };
            }
            return new RunResult
            {
                Warnings = tokens[2]?.Split('\n')?.ToList(),
                Output = tokens[0],
                Debug = tokens[1]
            };
        }
    }
}
