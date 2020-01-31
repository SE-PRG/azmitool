using azmi_main;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace azmi_commandline
{

    class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = ConfigureArguments();
            var parseResult = rootCommand.Invoke(args);
            Environment.Exit(parseResult);            
        }


        static RootCommand ConfigureArguments()
        {

            //
            // Create subcommands
            // using nuget from https://github.com/dotnet/command-line-api
            //

            var rootCommand = new RootCommand()
            {
                Description = "Command-line utility azmi stands for Azure Managed Identity.\n" +
                    "It is helping admins simplify common operations (reading and writing) on Azure resources.\n" +
                    "It is utilizing Azure AD authentication via user assigned managed identity.",
            };


            //
            // gettoken
            //

            var getTokenCommand = new Command("gettoken", "Obtain Azure authorization token.");
            var endpointOption = new Option(new String[] { "--endpoint", "-e" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Endpoint against which to authenticate. Examples: management, storage. Default 'management'",
                Required = false
            };
            getTokenCommand.AddOption(endpointOption);
            rootCommand.AddCommand(getTokenCommand);


            //
            // setblob
            //

            var setBlobCommand = new Command("setblob", "Write local file to storage account blob.");

            var fileOption = new Option(new String[] { "--file", "-f" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to local file which will be uploaded. Examples: /tmp/1.txt, ./1.xml",
                Required = true,
            };
            setBlobCommand.AddOption(fileOption);

            var containerOption = new Option(new String[] { "--container", "-c" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of container to which file will be uploaded. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = true
            };
            setBlobCommand.AddOption(containerOption);
            rootCommand.AddCommand(setBlobCommand);


            //
            // define actual subcommand handlers
            //

            getTokenCommand.Handler = CommandHandler.Create<string>((endpoint) =>
            {
                try {
                    Console.WriteLine(Operations.getToken());
                } catch (Exception ex) {
                    DisplayError("gettoken", ex);
                }
            });

            setBlobCommand.Handler = CommandHandler.Create<string, string>((file, container) =>
            {
                try {
                    Console.WriteLine(Operations.setBlob(file, container));
                } catch (Exception ex) {
                    DisplayError("setblob", ex);
                }
            });

            // return generated command
            return rootCommand;
        }

        private static void DisplayError(string subCommand, Exception ex)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"azmi {subCommand}: {ex.Message}");
            Console.ForegroundColor = oldColor;
            Environment.Exit(2);
            // invocation returns exit code 2, parser errors will return exit code 1
        }
    }
}