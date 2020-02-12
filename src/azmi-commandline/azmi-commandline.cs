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

            var shared_verboseOption = new Option(new String[] { "--verbose", "-v" })
            {
                Argument = new Argument<bool>("bool"),
                Description = "If enabled, commands will produce more verbose error output.",
                Required = false
            };
            var shared_identityOption = new Option(new String[] { "--identity", "-i" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Client or application ID of managed identity used to authenticate. Example: 117dc05c-4d12-4ac2-b5f8-5e239dc8bc54",
                Required = false
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
            getTokenCommand.AddOption(shared_identityOption);
            getTokenCommand.AddOption(shared_verboseOption);
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
            getBlobCommand.AddOption(shared_identityOption);
            getBlobCommand.AddOption(shared_verboseOption);

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
                Description = "URL of container to which file will be uploaded. Cannot be used together with --blob. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = false
            };
            var setBlob_blobOption = new Option(new String[] { "--blob", "-b" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of blob to which file will be uploaded. Cannot be used together with --container. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob.txt",
                Required = false
            };
            setBlobCommand.AddOption(setBlob_containerOption);
            setBlobCommand.AddOption(setBlob_blobOption);
            setBlobCommand.AddOption(shared_identityOption);
            setBlobCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(setBlobCommand);

            //
            // define actual subcommand handlers
            //

            getTokenCommand.Handler = CommandHandler.Create<string, string, bool>((endpoint, identity, verbose) =>
            {
                try
                {
                    Console.WriteLine(Operations.getToken(endpoint, identity));
                } catch (Exception ex)
                {
                    DisplayError("gettoken", ex, verbose);
                }
            });

            getBlobCommand.Handler = CommandHandler.Create<string, string, string, bool>((blob, file, identity, verbose) =>
            {
                try
                {
                    Console.WriteLine(Operations.getBlob(blob, file, identity));
                } catch (Exception ex)
                {
                    DisplayError("getblob", ex, verbose);
                }
            });

            setBlobCommand.Handler = CommandHandler.Create<string, string, string, string, bool>((file, blob, container, identity, verbose) =>
            {
                if (blob == null && container == null)
                {
                    throw new ArgumentNullException("blob, container", "You must specify either blob or container url");
                } else if (blob != null && container != null)
                {
                    throw new ArgumentException("Cannot use both container and blob url");
                } else
                {
                    try
                    {
                        if (container != null)
                        {
                            Console.WriteLine(Operations.setBlob_byContainer(file, container, identity));
                        } else
                        {
                            Console.WriteLine(Operations.setBlob_byBlob(file, blob, identity));
                        }                        
                    } catch (Exception ex)
                    {
                        DisplayError("setblob", ex, verbose);
                    }
                }
            });

            // return generated command
            return rootCommand;
        }

        private static void DisplayError(string subCommand, Exception ex, bool verbose)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"azmi {subCommand}: {ex.Message}");
            if (verbose)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Console.Error.WriteLine(ex.Message);
                }
            }
            Console.ForegroundColor = oldColor;
            Environment.Exit(2);
            // invocation returns exit code 2, parser errors will return exit code 1
        }
    }
}