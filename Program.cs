using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace TioTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected error: " + e);
                Environment.Exit(-1);
            }
        }

        private static void Execute(string[] args)
        {
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            ApplyCommandLineArguments(args, config);

            if (Directory.Exists(config.Test))
            {
                string[] files = Directory.GetFiles(config.Test, "*.json");
                Array.Sort(files);
                int counter = 0;
                foreach (string file in files)
                {
                    Console.Write($"[{++counter}/{files.Length}] ");
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

        private static void ApplyCommandLineArguments(string[] args, Config config)
        {
            CommandLineApplication cla = new CommandLineApplication(false);
            CommandOption url = cla.Option(
                "-u | --url",
                "The url to send our test for execution. RunUrl in config.json",
                CommandOptionType.SingleValue
            );
            CommandOption test = cla.Option(
                "-n | --test",
                "File or directory name with test(s). Files are expected to have *.json extension. Test in config.json",
                CommandOptionType.SingleValue
            );

            CommandOption trim = cla.Option(
                "-t | --trim",
                "Trim white spaces including new lines from results before comparing to expected output. Specify on or off. TrimWhitespacesFromResults in config.json",
                CommandOptionType.SingleValue
            );

            CommandOption dumpOnError = cla.Option(
                "-f | --dump-on-error",
                "Display Debug info and Warnings that came from the server if expected output does not match the execution result. Specify on or off. DisplayDebugInfoOnError in config.json",
                CommandOptionType.SingleValue
            );

            CommandOption dumpOnSuccess = cla.Option(
                "-s | --dump-on-success",
                "Display Debug info and Warnings that come from the server if expected output does match the execution result. Specify on or off. DisplayDebugInfoOnSuccess in config.json",
                CommandOptionType.SingleValue
            );

            cla.HelpOption("-? | -h | --help");
            cla.OnExecute(() =>
            {
                if (url.HasValue())
                {
                    config.RunUrl = url.Value();
                }
                if (test.HasValue())
                {
                    config.Test = test.Value();
                }
                if (trim.HasValue())
                {
                    SetBooleanOption(trim, cla, b => config.TrimWhitespacesFromResults = b);
                }
                if (dumpOnError.HasValue())
                {
                    SetBooleanOption(dumpOnError, cla, b => config.DisplayDebugInfoOnError = b);
                }
                if (dumpOnSuccess.HasValue())
                {
                    SetBooleanOption(dumpOnSuccess, cla, b => config.DisplayDebugInfoOnSuccess = b);
                }
                return 0;
            });

            try
            {
                cla.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                cla.ShowHelp();
            }
        }

        private static void SetBooleanOption(CommandOption trim, CommandLineApplication cla, Action<bool> set )
        {
            string[] yes = { "on", "true", "enable", "+", "yes" };
            string[] no = { "off", "false", "disable", "-", "no" };
            if (yes.Contains(trim.Value()))
            {
                set(true);
            }
            else if (no.Contains(trim.Value()))
            {
                set(false);
            }
            else
            {
                throw new CommandParsingException(cla, $"Unrecognized value {trim.Value()} of option {trim.LongName}");
            }
        }

        private static void RunTest(string file, Config config)
        {
            var name = file.EndsWith(".json") ? file.Substring(0, file.Length - ".json".Length) : file;
            name = Path.GetFileName(name);
            Console.Write($"{name}...");
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
            Console.Write($"\r{string.Empty.PadLeft(name.Length+3)}\r");
            if (test.Output == result.Output)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{name} - PASS ({time})");
                Console.ResetColor();
                if (config.DisplayDebugInfoOnSuccess)
                {
                    if (result.Warnings != null)
                    {
                        foreach (string warning in result.Warnings)
                        {
                            Console.WriteLine($"Warning: {warning}");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(result.Debug))
                    {
                        Console.WriteLine($"Debug {result.Debug}");
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{name} - FAIL ({time})");
                Console.ResetColor();
                if (config.DisplayDebugInfoOnError)
                {
                    Console.WriteLine($"Expected: {test.Output}");
                    Console.WriteLine($"Got: {result.Output}");
                    if (result.Warnings != null)
                    {
                        foreach (string warning in result.Warnings)
                        {
                            Console.WriteLine($"Warning: {warning}");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(result.Debug))
                    {
                        Console.WriteLine($"Debug {result.Debug}");
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
                Warnings = tokens[2]?.Split('\n').Where(x=>!string.IsNullOrWhiteSpace(x)).ToList(),
                Output = tokens[0],
                Debug = tokens[1]
            };
        }
    }
}
