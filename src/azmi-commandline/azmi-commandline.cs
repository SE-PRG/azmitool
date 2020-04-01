using azmi_main;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace azmi_commandline
{ 
    class Program
    {
        // https://github.com/dotnet/command-line-api/issues/458
        // CommandHandler.Create() does not support more than 7 parameters at the moment
        // Workaround: bind complex objects
        // TODO: This workaround/class should be cleaned up in future
        class getBlobsOptionsType
        {
            public string container { get; set; }
            public string directory { get; set; }
            public string identity { get; set; }
            public string prefix { get; set; }
            public string exclude { get; set; }
            public bool ifNewer { get; set; }
            public bool deleteAfterCopy { get; set; }
            public bool verbose { get; set; }
        }

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
                    "It is utilizing Azure AD authentication via user-assigned managed identity.\n" +
                    "Type azmi --help or azmi <command> --help for details on each command.",
            };

            var shared_verboseOption = new Option(new String[] { "--verbose", "-v" })
            {                
                Description = "Optional. If enabled, commands will produce more verbose error output.",
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

            var getTokenCommand = new Command("gettoken", "Obtains Azure authorization token for usage in other command line tools.");

            var getToken_endpointOption = new Option(new String[] { "--endpoint", "-e" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Endpoint against which to authenticate. Examples: management, storage. Default 'management'",
                Required = false
            };
            var getToken_JWTOption = new Option("--jwt-format")
            {                
                Description = "Optional. Print token in JSON Web Token (JWT) format.",
                Required = false
            };
            getTokenCommand.AddOption(getToken_endpointOption);
            getTokenCommand.AddOption(getToken_JWTOption);
            getTokenCommand.AddOption(shared_identityOption);
            getTokenCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(getTokenCommand);

            //
            // getblob
            //

            var getBlobCommand = new Command("getblob", "Downloads blob from storage account to local file.");

            var getBlob_blobOption = new Option(new String[] { "--blob", "-b" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of blob which will be downloaded. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob",
                Required = true
            };            
            var getBlob_fileOption = new Option(new String[] { "--file", "-f" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to local file to which content will be downloaded. Examples: /tmp/1.txt, ./1.xml",
                Required = true
            };
            var getBlob_ifNewerOption = new Option("--if-newer")
            {                
                Description = "Optional. Download a blob only if a newer version exists in a container.",
                Required = false
            };
            var getBlob_deleteAfterCopyOption = new Option("--delete-after-copy")
            {
                Description = "Optional. Successfully downloaded blob is removed from a container.",
                Required = false
            };
            getBlobCommand.AddOption(getBlob_blobOption);
            getBlobCommand.AddOption(getBlob_fileOption);
            getBlobCommand.AddOption(getBlob_ifNewerOption);
            getBlobCommand.AddOption(getBlob_deleteAfterCopyOption);
            getBlobCommand.AddOption(shared_identityOption);
            getBlobCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(getBlobCommand);

            //
            // getblobs
            //

            var getBlobsCommand = new Command("getblobs", "Downloads blobs from container to local directory.");

            var getBlobs_containerOption = new Option(new String[] { "--container", "-c" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of container blobs will be downloaded from. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = true
            };
            var getBlobs_directoryOption = new Option(new String[] { "--directory", "-d" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to a local directory to which blobs will be downloaded to. Examples: /home/avalanche/tmp/ or ./",
                Required = true
            };
            var getBlobs_prefixOption = new Option(new String[] { "--prefix", "-p" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Specifies a string that filters the results to return only blobs whose name begins with the specified prefix",
                Required = false
            };
            var getBlobs_excludeOption = new Option(new String[] { "--exclude", "-e" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Exclude blobs that match given regular expression.",
                Required = false
            };
            var getBlobs_ifNewerOption = new Option("--if-newer")
            {
                Description = "Optional. Download blobs only if newer versions exist in a container.",
                Required = false
            };
            var getBlobs_deleteAfterCopyOption = new Option("--delete-after-copy")
            {
                Description = "Optional. Successfully downloaded blobs are removed from a container.",
                Required = false
            };
            getBlobsCommand.AddOption(getBlobs_containerOption);
            getBlobsCommand.AddOption(getBlobs_directoryOption);
            getBlobsCommand.AddOption(getBlobs_prefixOption);
            getBlobsCommand.AddOption(getBlobs_excludeOption);
            getBlobsCommand.AddOption(getBlobs_ifNewerOption);
            getBlobsCommand.AddOption(getBlobs_deleteAfterCopyOption);
            getBlobsCommand.AddOption(shared_identityOption);
            getBlobsCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(getBlobsCommand);

            //
            // setblob
            //

            var setBlobCommand = new Command("setblob", "Writes local file to storage account blob.");

            var setBlob_fileOption = new Option(new String[] { "--file", "-f" })
            {
                Argument = new Argument<String>("string"),
                Description = "Path to local file which will be uploaded. Examples: /tmp/1.txt, ./1.xml",
                Required = true
            };            
            var setBlob_containerOption = new Option(new String[] { "--container", "-c" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. URL of container to which file will be uploaded. Cannot be used together with --blob. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = false
            };
            var setBlob_blobOption = new Option(new String[] { "--blob", "-b" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. URL of blob to which file will be uploaded. Cannot be used together with --container. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob.txt",
                Required = false
            };
            var setBlob_forceOption = new Option("--force")
            {                
                Description = "Optional. Overwrite existing blob in Azure.",
                Required = false
            };
            setBlobCommand.AddOption(setBlob_fileOption);
            setBlobCommand.AddOption(setBlob_containerOption);
            setBlobCommand.AddOption(setBlob_blobOption);
            setBlobCommand.AddOption(setBlob_forceOption);
            setBlobCommand.AddOption(shared_identityOption);
            setBlobCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(setBlobCommand);

            //
            // listblobs
            //

            var listBlobsCommand = new Command("listblobs", "List all blobs in container and send to output.");

            var listBlobs_containerOption = new Option(new String[] { "--container", "-c" })
            {
                Argument = new Argument<String>("string"),
                Description = "URL of container for which to list blobs. Example: https://myaccount.blob.core.windows.net/mycontainer",
                Required = true
            };
            var listBlobs_prefixOption = new Option(new String[] { "--prefix", "-p" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Specifies a string that filters the results to return only blobs whose name begins with the specified prefix",
                Required = false
            };
            var listBlobs_excludeOption = new Option(new String[] { "--exclude", "-e" })
            {
                Argument = new Argument<String>("string"),
                Description = "Optional. Exclude blobs that match given regular expression.",
                Required = false
            };
            listBlobsCommand.AddOption(listBlobs_containerOption);
            listBlobsCommand.AddOption(listBlobs_prefixOption);
            listBlobsCommand.AddOption(listBlobs_excludeOption);
            listBlobsCommand.AddOption(shared_identityOption);
            listBlobsCommand.AddOption(shared_verboseOption);

            rootCommand.AddCommand(listBlobsCommand);

            //
            // define actual subcommand handlers
            //

            Operations operations = new Operations();

            // gettoken
            getTokenCommand.Handler = CommandHandler.Create<string, string, bool, bool>((endpoint, identity, JWTFormat, verbose) =>
            {
                try
                {
                    Console.WriteLine(operations.getToken(endpoint, identity, JWTFormat));
                } catch (Exception ex)
                {
                    DisplayError("gettoken", ex, verbose);
                }
            });

            // getblob
            getBlobCommand.Handler = CommandHandler.Create<string, string, string, bool, bool, bool>((blob, file, identity, ifNewer, deleteAfterCopy, verbose) =>
            {
                try
                {
                    Console.WriteLine(operations.getBlob(blob, file, identity, ifNewer, deleteAfterCopy));
                } catch (Exception ex)
                {
                    DisplayError("getblob", ex, verbose);
                }
            });

            // getblobs
            getBlobsCommand.Handler = CommandHandler.Create<getBlobsOptionsType>(optionsType =>
            {
                try
                {
                    List<string> results = operations.getBlobs(optionsType.container, optionsType.directory, optionsType.identity, optionsType.prefix, optionsType.exclude, optionsType.ifNewer, optionsType.deleteAfterCopy);
                    if (results != null)
                    {
                        Console.WriteLine(String.Join("\n", results));
                    }
                }
                catch (Exception ex)
                {
                    DisplayError("getblobs", ex, optionsType.verbose);
                }
            });

            // setblob
            setBlobCommand.Handler = CommandHandler.Create<string, string, string, string, bool, bool>((file, blob, container, identity, force, verbose) =>
            {
                if (String.IsNullOrEmpty(blob) && String.IsNullOrEmpty(container))
                {
                    throw new ArgumentException("You must specify either blob or container url");
                }
                else if ( (!String.IsNullOrEmpty(blob)) && (!String.IsNullOrEmpty(container)))
                {
                    throw new ArgumentException("Cannot use both container and blob url");
                } 

                try
                {
                    Console.WriteLine(container != null
                        ? operations.setBlob_byContainer(file, container, identity, force)
                        : operations.setBlob_byBlob(file, blob, identity, force)
                        );
                } catch (Exception ex)
                {
                    DisplayError("setblob", ex, verbose);
                }
            });

            // listblobs
            listBlobsCommand.Handler = CommandHandler.Create<string, string, string, string, bool>((container, identity, prefix, exclude, verbose) =>
            {
                try
                {
                    List<string> blobsListing = operations.listBlobs(container, identity, prefix, exclude);
                    if (blobsListing != null)
                    {
                        Console.WriteLine(String.Join("\n", blobsListing));
                    }
                } catch (Exception ex)
                {
                    DisplayError("listblobs", ex, verbose);
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
            Console.Error.WriteLine(ex.GetType());
            while (verbose && ex.InnerException != null)
            {
                ex = ex.InnerException;
                Console.Error.WriteLine(ex.GetType());
                Console.Error.WriteLine(ex.Message);
            }
            Console.ForegroundColor = oldColor;
            Environment.Exit(2);
            // invocation returns exit code 2, parser errors will return exit code 1
        }
    }
}