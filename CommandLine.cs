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

        private static void SetBooleanOption(CommandOption trim, CommandLineApplication cla, Action<bool> set)
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
    }
}
