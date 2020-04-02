using azmi_main;
using System;
using System.Collections.Generic;
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
            // Create main command
            // using nuget from https://github.com/dotnet/command-line-api
            //

            var rootCommand = new RootCommand()
            {
                Description = "Command-line utility azmi stands for Azure Managed Identity.\n" +
                    "It is helping admins simplify common operations (reading and writing) on Azure resources.\n" +
                    "It is utilizing Azure AD authentication via user-assigned managed identity.\n" +
                    "Type azmi --help or azmi <command> --help for details on each command.",
            };

            //
            // subcommands
            //


            // common
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetToken, GetToken.AzmiArgumentsClass>());

            // blob
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<ListBlobs, ListBlobs.AzmiArgumentsClass>());
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetBlob, GetBlob.AzmiArgumentsClass>());
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetBlobs, GetBlobs.AzmiArgumentsClass>());
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<SetBlob, SetBlob.AzmiArgumentsClass>());

            // secret
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetSecret, GetSecret.AzmiArgumentsClass>());

            // return generated command
            return rootCommand;
        }
    }
}