using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace TioTests
{
    public class CommandLine
    {
        public static void ApplyCommandLineArguments(string[] args, Config config)
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

            CommandOption useConsoleCodes = cla.Option(
                "-d | --use-console-codes",
                "If on will use ANSI escape codes. Set to off when redirecting the output to a file. Specify on or off. UseConsoleCodes in config.json",
                CommandOptionType.SingleValue
            );

            CommandOption checkMissing = cla.Option(
                "-c | --check-missing-tests",
                "Provide url to tio language.json. Pass off to switch off. The command will print out any discrepancies between languages listed there and in the folder specified by -n | --test option",
                CommandOptionType.SingleValue
            );

            CommandOption retry = cla.Option(
                "-r | --retries",
                "Specify how many times to retry a test if connection failed",
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
                if (useConsoleCodes.HasValue())
                {
                    SetBooleanOption(useConsoleCodes, cla, b => config.UseConsoleCodes = b);
                }
                if (checkMissing.HasValue())
                {
                    bool? val = null;
                    SetBooleanOption(checkMissing, cla, b => val = b);
                    config.CheckUrl = val.HasValue && !val.Value ? null : checkMissing.Value();
                }
                if (retry.HasValue())
                {
                    int retries = 0;
                    if (int.TryParse(retry.Value(), out retries))
                    {
                        config.Retries = retries;    
                    }
                }
                return 0;
            });

            try
            {
                cla.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Logger.LogLine(ex.Message);
                cla.ShowHelp();
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
