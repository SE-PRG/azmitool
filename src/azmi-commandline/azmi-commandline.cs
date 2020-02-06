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
            var getToken_endpointOption = new Option(new String[] { "--endpoint", "-e" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Endpoint against which to authenticate. Examples: management, storage. Default 'management'",
                Required = false
            };
            getTokenCommand.AddOption(getToken_endpointOption);

            var getToken_identityOption = new Option(new String[] { "--identity", "-i" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Use particular MSI identity to authenticate. Application ID or Client ID. Example: 017dc05c-4d12-4ac2-b5f8-5e239dc8bc54",
                Required = false
            };
            getTokenCommand.AddOption(getToken_identityOption);

            rootCommand.AddCommand(getTokenCommand);

            //
            // getblob
            //

            var getBlobCommand = new Command("getblob", "Downloads storage account blob to local file.");

            var getBlob_blobOption = new Option(new String[] { "--blob", "-b" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of blob which will be downloaded. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob",
                Required = true
            };
            getBlobCommand.AddOption(getBlob_blobOption);

            var getBlob_fileOption = new Option(new String[] { "--file", "-f" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to local file to which content will be downloaded. Examples: /tmp/1.txt, ./1.xml",
                Required = true
            };
            getBlobCommand.AddOption(getBlob_fileOption);

            var getBlob_identityOption = new Option(new String[] { "--identity", "-i" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Use particular MSI identity to authenticate. Application ID or Client ID. Example: 017dc05c-4d12-4ac2-b5f8-5e239dc8bc54",
                Required = false
            };
            getBlobCommand.AddOption(getBlob_identityOption);

            rootCommand.AddCommand(getBlobCommand);

            //
            // setblob
            //

            var setBlobCommand = new Command("setblob", "Write local file to storage account blob.");

            var setBlob_fileOption = new Option(new String[] { "--file", "-f" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to local file which will be uploaded. Examples: /tmp/1.txt, ./1.xml",
                Required = true
            };
            setBlobCommand.AddOption(setBlob_fileOption);

            var setBlob_containerOption = new Option(new String[] { "--container", "-c" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of container to which file will be uploaded. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = true
            };
            setBlobCommand.AddOption(setBlob_containerOption);

            var setBlob_identityOption = new Option(new String[] { "--identity", "-i" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Use particular MSI identity to authenticate. Application ID or Client ID. Example: 017dc05c-4d12-4ac2-b5f8-5e239dc8bc54",
                Required = false
            };
            setBlobCommand.AddOption(setBlob_identityOption);

            rootCommand.AddCommand(setBlobCommand);

            //
            // define actual subcommand handlers
            //

            getTokenCommand.Handler = CommandHandler.Create<string, string>((endpoint, identity) =>
            {
                try {
                    Console.WriteLine(Operations.getToken(endpoint, identity));
                } catch (Exception ex) {
                    DisplayError("gettoken", ex);
                }
            });

            getBlobCommand.Handler = CommandHandler.Create<string, string, string>((blob, file, identity) =>
            {
                try
                {
                    Console.WriteLine(Operations.getBlob(blob, file, identity));
                } catch (Exception ex)
                {
                    DisplayError("getblob", ex);
                }
            });

            setBlobCommand.Handler = CommandHandler.Create<string, string, string>((file, container, identity) =>
            {
                try
                {
                    Console.WriteLine(Operations.setBlob(file, container, identity));
                } catch (Exception ex)
                {
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