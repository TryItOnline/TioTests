using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            string configPath = "config.json";
            Config config = File.Exists(configPath) ?
                JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)) :
                new Config { TrimWhitespacesFromResults = true, DisplayDebugInfoOnError = true, UseConsoleCodes = true };

            CommandLine.ApplyCommandLineArguments(args, config);

            if (Directory.Exists(config.Test))
            {
                if (!string.IsNullOrWhiteSpace(config.CheckUrl))
                {
                    CheckPath(config.CheckUrl,config.Test, config.UseConsoleCodes);
                }
                string[] files = Directory.GetFiles(config.Test, "*.json");
                Array.Sort(files);
                int counter = 0;
                foreach (string file in files)
                {
                    //Logger.Log($"[{++counter}/{files.Length}] ");
                    TestRunner.RunTest(file, config, $"[{++counter}/{files.Length}]");
                }
            }
            else if (File.Exists(config.Test))
            {
                TestRunner.RunTest(config.Test, config,"[1/1]");
            }
            else if (File.Exists(config.Test + ".json"))
            {
                TestRunner.RunTest(config.Test + ".json", config,"1/1");
            }
            else
            {
                Logger.LogLine($"Tests '{config.Test}' not found");
            }
        }

        private static void CheckPath(string checkUrl, string testFolder, bool useConsoleCodes)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response =  client.GetAsync(checkUrl).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            var j = (JObject)JsonConvert.DeserializeObject(content);
            var listFromUrl = j["practical"]["byId"]
                .Select(x => (string)((dynamic)x).Name)
                .Union(j["recreational"]["byId"]
                    .Select(x => (string)((dynamic)x).Name))
                .OrderBy(x => x)
                .ToList();    
            
            var listFromFolder = Directory.GetFiles(testFolder, "*.json")
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .OrderBy(x=>x)
                .ToList();

            var missing = listFromUrl.Except(listFromFolder).ToList();
            var extra = listFromFolder.Except(listFromUrl).ToList();

            Logger.LogLine($"Checking for missing tests...");
            Logger.LogLine($"Missing languages: {missing.Count}");
            foreach (string language in missing)
            {
                if (useConsoleCodes)  Console.ForegroundColor = ConsoleColor.Red;
                Logger.LogLine($"MISSING: {language}");
                if (useConsoleCodes)  Console.ResetColor();
            }

            if (extra.Count > 0)
            {
                Logger.LogLine($"There are {extra.Count} extra tests");
                foreach (string language in extra)
                {
                    Logger.LogLine($"EXTRA: {language}");
                }
            }
            if (extra.Count == 0 && missing.Count == 0)
            {
                if (useConsoleCodes)  Console.ForegroundColor = ConsoleColor.Green;
                Logger.LogLine("PASS: Tests are up to date");
                if (useConsoleCodes)  Console.ResetColor();
            }
        }
    }
}
