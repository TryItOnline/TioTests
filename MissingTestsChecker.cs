using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TioTests
{
    public static class MissingTestsChecker
    {
        public static void Check(string checkUrl, string testFolder, bool useConsoleCodes)
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            string s;
            try
            {
                HttpResponseMessage response = client.GetAsync(checkUrl).Result;
                s = response.Content.ReadAsStringAsync().Result;
            }
            catch (AggregateException ex)
            {
                if (useConsoleCodes) Console.ForegroundColor = ConsoleColor.Red;
                Logger.LogLine($"ERROR: Could not check for missing languages {ex}");
                if (useConsoleCodes) Console.ResetColor();
                return;
            }

            var j = (JObject)JsonConvert.DeserializeObject(s);
            var listFromUrl = j["practical"]["byId"]
                .Select(x => (string)((dynamic)x).Name)
                .Union(j["recreational"]["byId"]
                    .Select(x => (string)((dynamic)x).Name))
                .OrderBy(x => x)
                .ToList();

            var listFromFolder = Directory.GetFiles(testFolder, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x)
                .ToList();

            var missing = listFromUrl.Except(listFromFolder).ToList();
            var extra = listFromFolder.Except(listFromUrl).ToList();

            Logger.LogLine("Checking for missing tests...");
            Logger.LogLine($"Missing languages: {missing.Count}");
            foreach (string language in missing)
            {
                if (useConsoleCodes) Console.ForegroundColor = ConsoleColor.Red;
                Logger.LogLine($"MISSING: {language}");
                if (useConsoleCodes) Console.ResetColor();
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
                if (useConsoleCodes) Console.ForegroundColor = ConsoleColor.Green;
                Logger.LogLine("PASS: Tests are up to date");
                if (useConsoleCodes) Console.ResetColor();
            }
        }
    }
}
