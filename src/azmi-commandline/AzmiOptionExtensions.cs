using azmi_main;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace azmi_commandline
{
    public static class AzmiOptionExtensions
    {
        public static String[] OptionNames(this AzmiOption option)
        {
            if (option.shortName != null)
            {
                return new String[] { $"--{option.name}", $"-{option.shortName}" };
            } else
            {
                return new String[] { $"--{option.name}" };
            }
        }

        public static Argument OptionArgument(this AzmiOption option)
        {
            if (option.type == AcceptedTypes.stringType)
            {
                return new Argument<string>("string");
            } else
            {
                return new Argument<bool>("bool");
            }
        }

        public static string OptionDescription(this AzmiOption option)
        {
            return 
                (option.required 
                    ? "Required. " 
                    : "Optional. "
                ) 
                + option.description;
        }

        public static Option ToOption(this AzmiOption option)
        {
            return new Option(option.OptionNames())
            {
                Argument = option.OptionArgument(),
                Description = option.OptionDescription(),
                Required = option.required
            };
        }

        public static Command ToCommand<T,TOptions> () where T: IAzmiCommand, new()
        {

            T cmd = new T();
            var commandLineSubCommand = new Command(cmd.Name(), cmd.Description());

            foreach (var op in cmd.AzmiOptions())
            {
                commandLineSubCommand.AddOption(op.ToOption());
                // TODO: Implement sorting: 1st required, then strings, then alphabet
            }
            commandLineSubCommand.Handler = CommandHandler.Create<TOptions>(
                op => Console.WriteLine(cmd.Execute(op)));

            return commandLineSubCommand;
        }

    }
}
