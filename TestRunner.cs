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
        public static void RunTest(string file, Config config, string counter)
        {
            var name = file.EndsWith(".json") ? file.Substring(0, file.Length - ".json".Length) : file;
            name = Path.GetFileName(name);
            if (config.UseConsoleCodes) Logger.Log($"{counter} {name}...");
            TestDescription test = JsonConvert.DeserializeObject<TestDescription>(Encoding.UTF8.GetString(File.ReadAllBytes(file)));
            
            RunResult result;
            string time;
            int retried = 0;
            while(true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                result = Execute(test.GetInputBytes(), config.RunUrl);
                sw.Stop();
                time = TimeFormatter.LargestIntervalWithUnits(sw.Elapsed);
                if (!result.HttpFailure || retried++ >= config.Retries)
                {
                    break;
                }
                if (config.UseConsoleCodes)
                {
                    //Logger.Log($"\r{string.Empty.PadLeft(name.Length + 40)}\r", true);
                }
                if (config.UseConsoleCodes)  Console.ForegroundColor = ConsoleColor.Red;
                if (config.UseConsoleCodes)
                {
                    Logger.Log($"{name} - FAIL ({time}) Retrying({retried})...\x1b[K" );

                }
                else
                {
                    Logger.LogLine($"{counter} {name} - FAIL ({time}) Retrying({retried})...");
                }
                if (config.UseConsoleCodes)  Console.ResetColor();
            }

            if (config.TrimWhitespacesFromResults)
            {
                result.Output = result.Output?.Trim("\n\r\t ".ToCharArray());
            }
            if (config.UseConsoleCodes)
            {
                //Logger.Log($"\r{string.Empty.PadLeft(name.Length + 23)}\r", true);
                Logger.Log($"\r", true);
            }
            if (test.Output == result.Output)
            {
                if (config.UseConsoleCodes) Console.ForegroundColor = ConsoleColor.Green;
                Logger.LogLine(config.UseConsoleCodes ? $"{name} - PASS ({time})\x1b[K" : $"{counter} {name} - PASS ({time})");
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
                Logger.LogLine(config.UseConsoleCodes ? $"{name} - FAIL ({time})\x1b[K" : $"{counter} {name} - FAIL ({time})");
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


        //private static void Decode(byte[] toDecompress)
        //{
        //    byte[] header = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x03};

        //    byte[] n = new byte[toDecompress.Length + header.Length];
        //    Array.Copy(header, 0, n, 0, header.Length);
        //    Array.Copy(toDecompress, 0, n, header.Length, toDecompress.Length);

        //    using (MemoryStream compressed = new MemoryStream(n))
        //    using (GZipStream compressor = new GZipStream(compressed, CompressionMode.Decompress))
        //    {
        //        MemoryStream res = new MemoryStream();
        //        byte[] buffer = new byte[1024];
        //        int nRead;
        //        while ((nRead = compressor.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            res.Write(buffer, 0, nRead);
        //        }
        //        res.Flush();
        //        string s = Encoding.UTF8.GetString(res.ToArray());
        //        s.ToString();
        //    }
        //}

        private static RunResult Execute(byte[] test, string configRunUrl)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(configRunUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            HttpContent z = new ByteArrayContent(test);
            string s;
            try
            {
                HttpResponseMessage response = client.PostAsync(configRunUrl, z).Result;
                s = response.Content.ReadAsStringAsync().Result;
            }
            catch (AggregateException ex)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't connect to [{configRunUrl}]",$"{ex}" },
                    HttpFailure = true
                };
            }
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
