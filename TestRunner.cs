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
        public static void RunTestsBatchLocal(string[] files, Config config)
        {
            List<string> expectedOutput;
            byte[] payload = PrepareBatch(files, config.DebugDump, out expectedOutput);

            int numberOfTests = files.Length;

            Logger.LogLine($"Running a batch of {numberOfTests} tests");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            RunResult result = ExecuteLocal(payload, Utility.GetProcess(config), Utility.GetArenaHost(config), numberOfTests * 50, numberOfTests * 50 + 5, config.DebugDump);
            sw.Stop();
            string time = TimeFormatter.FormatTime(sw.Elapsed);

            if (numberOfTests != result.Output?.Count || result.Output?.Count != result.Debug?.Count)
            {
                Logger.LogLine($"Error: Unexpected response. Tests:{numberOfTests}, Output:{result.Output?.Count}, Debug: {result.Debug?.Count}");
                ShowWarnings(result.Warnings);
                return;
            }

            // In the batch mode backed returns a single set of warnings for all the tests. Display them here
            if (config.DisplayDebugInfoOnSuccess)
            {
                ShowWarnings(result.Warnings);
            }

            int successes = 0;
            for (int i = 0; i < numberOfTests; i++)
            {
                DisplayTestResultParams p = new DisplayTestResultParams
                {
                    TestName = GetTestName(files[i]),
                    ExpectedOutput = expectedOutput[i],
                    Output = config.TrimWhitespacesFromResults ? Utility.TrimWhitespaces(result.Output[i]) : result.Output[i],
                    Debug = result.Debug[i]
                };
                DisplayTestResult(p, config);
                if (p.Success)
                {
                    successes++;
                }
            }
            Logger.LogLine($"Elapsed: {time}");
            Logger.LogLine($"Result: {successes} succeeded, {numberOfTests - successes} failed");
        }

        private static void ShowWarnings(List<string> warnings)
        {
            if (warnings != null)
            {
                foreach (string warning in warnings)
                {
                    Logger.LogLine($"Warning: {warning}");
                }
            }
        }

        private static byte[] PrepareBatch(string[] files, bool dump, out List<string> expectedOutput)
        {
            // Parse files with test definitions, prepare request to run the test and collect 
            // expected test results
            byte[] payload;
            expectedOutput = new List<string>();
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (string file in files)
                {
                    TestDescription test = JsonConvert.DeserializeObject<TestDescription>(Encoding.UTF8.GetString(File.ReadAllBytes(file)));
                    ms.Write(test.GetInputBytes());
                    expectedOutput.Add(test.Output);
                }
                payload = CompressAndDump(ms.ToArray(), dump, "Local");
            }
            return payload;
        }

        public static bool RunSingleTest(string file, Config config, string counter)
        {
            string testName = GetTestName(file);
            if (config.UseConsoleCodes) Logger.Log($"{counter} {testName}...");

            TestDescription test = JsonConvert.DeserializeObject<TestDescription>(Encoding.UTF8.GetString(File.ReadAllBytes(file)));

            RunResult result;
            string time;
            int retried = 0;

            // This is the retry loop for flaky HTTP connection. Note that local runs are never over HTTP, so they are never retried 
            while (true)
            {
                byte[] compressed = CompressAndDump(test.GetInputBytes(), config.DebugDump, config.LocalRun ? "Local" : "Remote");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                result = config.LocalRun
                    ? ExecuteLocal(compressed, Utility.GetProcess(config), Utility.GetArenaHost(config), 60, 65, config.DebugDump)
                    : ExecuteRemote(compressed, config.RunUrl, config.DebugDump);
                sw.Stop();

                time = TimeFormatter.FormatTime(sw.Elapsed);
                if (!result.HttpFailure || retried++ >= config.Retries)
                {
                    break;
                }
                if (config.UseConsoleCodes)
                {
                    Utility.ClearLine(testName);
                }
                if (config.UseConsoleCodes) Console.ForegroundColor = ConsoleColor.Red;
                if (config.UseConsoleCodes)
                {
                    Logger.Log($"{testName} - FAIL ({time}) Retrying({retried})...");

                }
                else
                {
                    Logger.LogLine($"{counter} {testName} - FAIL ({time}) Retrying({retried})...");
                }
                if (config.UseConsoleCodes) Console.ResetColor();
            }

            if (config.UseConsoleCodes)
            {
                Utility.ClearLine(testName);
            }

            DisplayTestResultParams p = new DisplayTestResultParams
            {
                TestName = testName,
                ExpectedOutput = test.Output,
                Output = config.TrimWhitespacesFromResults ? Utility.TrimWhitespaces(result.Output[0]) : result.Output[0],
                Debug = result.Debug[0],
                Time = time,
                Counter = counter,
                Warnings = result.Warnings
            };
            DisplayTestResult(p, config);
            return p.Success;
        }

        private static string GetTestName(string file)
        {
            var name = file.EndsWith(".json") ? file.Substring(0, file.Length - ".json".Length) : file;
            name = Path.GetFileName(name);
            return name;
        }

        private static void DisplayTestResult(DisplayTestResultParams p, Config config)
        {
            if (config.UseConsoleCodes) Console.ForegroundColor = p.Color;
            if (!config.BatchMode) Logger.LogLine(config.UseConsoleCodes ? $"{p.TestName} - {p.Result} ({p.Time})" : $"{p.Counter} {p.TestName} - {p.Result} ({p.Time})");
            if (config.BatchMode && (!config.Quiet || config.DisplayDebugInfoOnError)) Logger.LogLine($"{p.TestName} - {p.Result}");
            if (config.UseConsoleCodes) Console.ResetColor();
            if (p.Success ? config.DisplayDebugInfoOnSuccess : config.DisplayDebugInfoOnError)
            {
                if (!p.Success)
                {
                    Logger.LogLine($"Expected: {p.ExpectedOutput}");
                    Logger.LogLine($"Got: {p.Output}");
                }
                if (!config.BatchMode)
                {
                    ShowWarnings(p.Warnings);
                }
                if (!string.IsNullOrWhiteSpace(p.Debug))
                {
                    Logger.LogLine($"Debug {p.Debug}");
                }
            }
        }

        private static RunResult ExecuteLocal(byte[] test, string process, string arenaHost, int softTimeOut, int hardTimeOut, bool dump)
        {
            ProcessStartInfo si = new ProcessStartInfo(process)
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            si.Environment["PATH_INFO"] = "/api/no-cache/";
            si.Environment["REQUEST_METHOD"] = "POST";
            si.Environment["SSH_USER_HOST"] = arenaHost;
            si.Environment["TIMEOUT_HARD"] = hardTimeOut.ToString();
            si.Environment["TIMEOUT_SOFT"] = softTimeOut.ToString();
            Process p = new Process { StartInfo = si };
            p.Start();
            p.StandardInput.BaseStream.Write(test);
            p.StandardInput.Dispose();
            byte[] result;
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int i = p.StandardOutput.BaseStream.ReadByte();
                    if (i == -1) break;
                    ms.WriteByte((byte)i);
                }
                result = ms.ToArray();
            }

            if (dump) Utility.Dump("Local raw response", result);

            byte[] endOfHeaders = { 0x0A, 0x0A };
            int offset = Utility.SearchBytes(result, endOfHeaders);

            if (offset < 0)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't parse response from {process}", $"{Encoding.UTF8.GetString(result)}" }
                };
            }

            offset += endOfHeaders.Length;
            byte[] raw = new byte[result.Length - offset];
            Array.Copy(result, offset, raw, 0, raw.Length);
            byte[] decoded = Compression.Decompress(raw);
            if (dump) Utility.Dump("Local decoded response", decoded);
            return ResponseToRunResult(decoded);
        }

        private static RunResult ExecuteRemote(byte[] test, string configRunUrl, bool dump)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(configRunUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            HttpContent z = new ByteArrayContent(test);
            byte[] b;
            try
            {
                HttpResponseMessage response = client.PostAsync(configRunUrl, z).Result;
                b = response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (AggregateException ex)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't connect to [{configRunUrl}]", $"{ex}" },
                    HttpFailure = true
                };
            }
            if (dump) Utility.Dump("Remote response", b);

            return ResponseToRunResult(b);
        }

        private static byte[] CompressAndDump(byte[] data, bool dump, string marker)
        {
            if (dump) Utility.Dump($"{marker} request raw", data);
            byte[] payload = Compression.Compress(data);
            if (dump) Utility.Dump($"{marker} request compressed", payload);
            return payload;
        }

        private static RunResult ResponseToRunResult(byte[] response)
        {
            string s = Encoding.UTF8.GetString(response);
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
            if (tokens.Length < 3 || tokens.Length % 2 != 1)
            {
                return new RunResult
                {
                    Warnings = new List<string> { $"Can't parse result [{s}]" }
                };
            }
            return new RunResult
            {
                Warnings = tokens[tokens.Length - 1]?.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                Output = tokens.Take(tokens.Length / 2).ToList(),
                Debug = tokens.Skip(tokens.Length / 2).Take(tokens.Length / 2).ToList()
            };
        }
    }
}
