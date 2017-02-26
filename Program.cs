using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
                Logger.LogLine("Unexpected error: " + e);
                Environment.Exit(-1);
            }
        }

        private static void Execute(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string configPath = "config.json";
            Config config = File.Exists(configPath) ?
                JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)) :
                new Config { TrimWhitespacesFromResults = true, DisplayDebugInfoOnError = true, UseConsoleCodes = true };

            CommandLine.ApplyCommandLineArguments(args, config);

            if (Directory.Exists(config.Test))
            {
                if (!string.IsNullOrWhiteSpace(config.CheckUrl))
                {
                    MissingTestsChecker.Check(config.CheckUrl,config.Test, config.UseConsoleCodes);
                }
                string[] files = Directory.GetFiles(config.Test, "*.json");
                Array.Sort(files);
                int counter = 0;
                int success = 0;
                if (!config.LocalRun || !config.BatchMode)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    foreach (string file in files)
                    {
                        if (TestRunner.RunSingleTest(file, config, $"[{++counter}/{files.Length}]"))
                        {
                            success++;
                        }
                    }
                    sw.Stop();
                    string time = TimeFormatter.FormatTime(sw.Elapsed);
                    Logger.LogLine($"Elapsed: {time}");
                    Logger.LogLine($"Result: {success} succeeded, {files.Length - success} failed");
                }
                else
                {
                    TestRunner.RunTestsBatchLocal(files,config);
                }
            }
            else if (File.Exists(config.Test))
            {
                TestRunner.RunSingleTest(config.Test, config,"[1/1]");
            }
            else if (File.Exists(config.Test + ".json"))
            {
                TestRunner.RunSingleTest(config.Test + ".json", config,"1/1");
            }
            else
            {
                Logger.LogLine($"Tests '{config.Test}' not found");
            }
        }
    }
}
