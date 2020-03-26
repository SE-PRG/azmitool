using azmi_main;

using System;
using System.CommandLine;


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
            var tmpDesc = option.description;
            if (option.required == false)
            {
                tmpDesc = "Optional. " + tmpDesc;
            };
            return tmpDesc;
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
    }
}
