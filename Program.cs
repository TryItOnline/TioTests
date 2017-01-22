using System;
using System.IO;
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
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            CommandLine.ApplyCommandLineArguments(args, config);

            if (Directory.Exists(config.Test))
            {
                string[] files = Directory.GetFiles(config.Test, "*.json");
                Array.Sort(files);
                int counter = 0;
                foreach (string file in files)
                {
                    Logger.Log($"[{++counter}/{files.Length}] ");
                    TestRunner.RunTest(file, config);
                }
            }
            else if (File.Exists(config.Test))
            {
                TestRunner.RunTest(config.Test, config);
            }
            else if (File.Exists(config.Test + ".json"))
            {
                TestRunner.RunTest(config.Test + ".json", config);
            }
            else
            {
                Logger.LogLine($"{config.Test} not found");
            }
        }
    }
}
