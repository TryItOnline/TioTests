using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace TioTests
{
    public class CommandLine
    {
        public static bool ApplyCommandLineArguments(string[] args, Config config)
        {
            CommandLineApplication cla = new CommandLineApplication(false);
            CommandOption url = cla.Option(
                "-u | --url",
                "RunUrl. The url to send our test for execution. ",
                CommandOptionType.SingleValue
            );
            CommandOption test = cla.Option(
                "-n | --test",
                "TestPath. File or directory name with test(s). Files are expected to have *.json extension.",
                CommandOptionType.SingleValue
            );

            CommandOption trim = cla.Option(
                "-t | --trim",
                "TrimWhitespacesFromResults. Trim white spaces including new lines from results before comparing to expected output. Specify on or off.",
                CommandOptionType.SingleValue
            );

            CommandOption dumpOnError = cla.Option(
                "-f | --dump-on-error",
                "DisplayDebugInfoOnError. Display Debug info and Warnings that came from the server if expected output does not match the execution result. Specify on or off.",
                CommandOptionType.SingleValue
            );

            CommandOption dumpOnSuccess = cla.Option(
                "-s | --dump-on-success",
                "DisplayDebugInfoOnSuccess. Display Debug info and Warnings that come from the server if expected output does match the execution result. Specify on or off.",
                CommandOptionType.SingleValue
            );

            CommandOption useConsoleCodes = cla.Option(
                "-d | --use-console-codes",
                "UseConsoleCodes. If on will use ANSI escape codes. Set to off when redirecting the output to a file. Specify on or off.",
                CommandOptionType.SingleValue
            );

            CommandOption checkMissing = cla.Option(
                "-c | --check-missing-tests",
                "CheckUrl. Provide url to tio language.json. Pass off to switch off. The command will print out any discrepancies between languages listed there and in the folder specified by -n | --test option",
                CommandOptionType.SingleValue
            );

            CommandOption retry = cla.Option(
                "-r | --retries",
                "Retries. Specify how many times to retry a test if connection failed",
                CommandOptionType.SingleValue
            );

            CommandOption localRun = cla.Option(
                "-l | --local-run",
                "LocalRun. Instead of running the command against http url supplied with -u run against local backend. Pass on to switch on. Pass path to local backend with -p. Pass ArenaHost with -z.",
                CommandOptionType.SingleValue
            );

            CommandOption localRoot = cla.Option(
                "-o | --local-root",
                "LocalRoot. Specifies the main server directory (/srv). Used in local run mode, if either or both -p and -z are not provided. See their respective descriptions",
                CommandOptionType.SingleValue
            );

            CommandOption localProcess = cla.Option(
                "-p | --local-process",
                "LocalProcess. Used in local run mode. Specifies the process path to run tests against. If not specified, will combine path provided by -o and 'tio.run/cgi-bin/run'",
                CommandOptionType.SingleValue
            );

            CommandOption arenaHost = cla.Option(
                "-z | --arena-host",
                "ArenaHost. Used in local run mode. Specifies the arena user and host to run tests against. If not specified, will use path provided by -o to look up the value in 'etc/run'",
                CommandOptionType.SingleValue
            );

            CommandOption batch = cla.Option(
                "-b | --batch-mode",
                "BatchMode .Used in local run mode. Batch up all the tests in a single call. There will be no progress indicator when tests are run in this mode. Pass off to switch off.",
                CommandOptionType.SingleValue
            );

            CommandOption quiet = cla.Option(
                "-q | --quiet",
                "Quiet. Used in batch mode. Suppresses results of individual tests, only displaying summary. -f or -s override this for failed and successful test respectively. Pass on to switch on.",
                CommandOptionType.SingleValue
            );

            CommandOption dump = cla.Option(
                "-x | --debug-dump",
                "DebugDumpFile. Append dumps of binary streams that are being sent and received to the file specified.",
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
                    config.TestPath = test.Value();
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
                if (useConsoleCodes.HasValue())
                {
                    SetBooleanOption(useConsoleCodes, cla, b => config.UseConsoleCodes = b);
                }
                if (checkMissing.HasValue())
                {
                    bool? val = null;
                    SetBooleanOption(checkMissing, cla, b => val = b, false);
                    config.CheckUrl = val != null && !val.Value ? null : checkMissing.Value();
                }
                if (retry.HasValue())
                {
                    int retries;
                    if (int.TryParse(retry.Value(), out retries))
                    {
                        config.Retries = retries;    
                    }
                }
                if (localRun.HasValue())
                {
                    SetBooleanOption(localRun, cla, b => config.LocalRun = b);
                }
                if (localRoot.HasValue())
                {
                    config.LocalRoot = url.Value();
                }
                if (localProcess.HasValue())
                {
                    config.LocalProcess = url.Value();
                }
                if (arenaHost.HasValue())
                {
                    config.ArenaHost = arenaHost.Value();
                }
                if (batch.HasValue())
                {
                    SetBooleanOption(batch, cla, b => config.BatchMode = b);
                }
                if (quiet.HasValue())
                {
                    SetBooleanOption(quiet, cla, b => config.Quiet = b);
                }
                if (dump.HasValue())
                {
                    bool? val = null;
                    SetBooleanOption(dump, cla, b => val = b, false);
                    config.DebugDumpFile = val != null && !val.Value ? null : dump.Value();
                }
                return 1;
            });

            try
            {
                return cla.Execute(args) == 1;
            }
            catch (CommandParsingException ex)
            {
                Logger.LogLine(ex.Message);
                cla.ShowHelp();
                return false;
            }
        }

        private static void SetBooleanOption(CommandOption trim, CommandLineApplication cla, Action<bool> set, bool throwIfDoesNotMatch = true)
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
                if (throwIfDoesNotMatch)
                {
                    throw new CommandParsingException(cla, $"Unrecognized value {trim.Value()} of option {trim.LongName}");
                }
            }
        }
    }
}
