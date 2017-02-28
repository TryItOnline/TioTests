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
                new Config
                {
                    RunUrl = "https://backend.tryitonline.net/run/api/no-cache/",
                    CheckUrl = "https://tryitonline.net/languages.json",
                    TestPath = Path.Combine(Utility.GetAncestor(AppContext.BaseDirectory,3),"HelloWorldTests"),
                    TrimWhitespacesFromResults = true,
                    DisplayDebugInfoOnError = true,
                    UseConsoleCodes = true,
                    BatchMode = true,
                    LocalRoot = "/srv"                     
                };

            if (!CommandLine.ApplyCommandLineArguments(args, config))
            {
                return;
            }

            if (Directory.Exists(config.TestPath))
            {
                if (!string.IsNullOrWhiteSpace(config.CheckUrl))
                {
                    MissingTestsChecker.Check(config.CheckUrl,config.TestPath, config.UseConsoleCodes);
                }
                string[] files = Directory.GetFiles(config.TestPath, "*.json");
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
            else if (File.Exists(config.TestPath))
            {
                TestRunner.RunSingleTest(config.TestPath, config,"[1/1]");
            }
            else if (File.Exists(config.TestPath + ".json"))
            {
                TestRunner.RunSingleTest(config.TestPath + ".json", config,"1/1");
            }
            else
            {
                Logger.LogLine($"Tests '{config.TestPath}' not found");
            }
        }
    }
}
