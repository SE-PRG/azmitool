using azmi_main;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace azmi_commandline
{
    internal static class AzmiCommandLineExtensions
    {
        internal static String[] OptionNames(this AzmiArgument option)
        {
            if (option.alias != null)
            {
                return new String[] { $"--{option.name}", $"-{option.alias}" };
            } else
            {
                return new String[] { $"--{option.name}" };
            }
        }

        internal static Argument OptionArgument(this AzmiArgument option)
        {
            switch (option.type)
            {
                case ArgType.str: return new Argument<string>("string");
                case ArgType.flag: return new Argument<bool>("bool");
                case ArgType.url: return new Argument<string>("url");

                default: throw new ArgumentException($"Unsupported option type: {option.type}");
            }
        }

        internal static string OptionDescription(this AzmiArgument option)
        {
            return
                (option.required
                    ? "Required. "
                    : "Optional. "
                )
                + option.description;
        }

        internal static Option ToOption(this AzmiArgument option)
        {
            return new Option(option.OptionNames())
            {
                Argument = option.OptionArgument(),
                Description = option.OptionDescription(),
                Required = option.required
            };
        }

        internal static Command ToCommand<T, TOptions>()
            where T : IAzmiCommand, new()
            where TOptions : SharedAzmiArgumentsClass
        {

            T cmd = new T();
            var commandLineSubCommand = new Command(cmd.Definition().name, cmd.Definition().description);

            foreach (var op in cmd.Definition().arguments)
            {
                commandLineSubCommand.AddOption(op.ToOption());
            }
            commandLineSubCommand.Handler = CommandHandler.Create<TOptions>(
                op => {
                    try {
                        cmd.Execute(op).WriteLines();
                    } catch (Exception ex) {
                        DisplayError(cmd.Definition().name, ex, op.verbose);
                    } });

            return commandLineSubCommand;
        }

        internal static void DisplayError(string subCommand, Exception ex, bool verbose)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"azmi {subCommand}: [{ex.GetType()}] {ex.Message}");
            while (verbose && ex.InnerException != null)
            {
                ex = ex.InnerException;
                Console.Error.WriteLine($"---\n[{ex.GetType()}] {ex.Message}");
            }
            Console.ForegroundColor = oldColor;
            Environment.Exit(2);
            // invocation returns exit code 2, parser errors will return exit code 1
        }

        internal static void WriteLines(this List<string> str)
        {
            if (str != null)
            {
                str.ForEach(l => Console.WriteLine(l));
            }
        }
    }
}
