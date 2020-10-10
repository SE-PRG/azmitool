using azmi_main;
using System;
using System.CommandLine;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-commandline-tests")]

namespace azmi_commandline
{
    static class Program
    {

        static async Task Main(string[] args)
        {
            var rootCommand = ConfigureArguments();
            var parseResult = await rootCommand.InvokeAsync(args).ConfigureAwait(false);
            Environment.Exit(parseResult);
        }

        internal static RootCommand ConfigureArguments()
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
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<SetBlobs, SetBlobs.AzmiArgumentsClass>());

            // secret
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetSecret, GetSecret.AzmiArgumentsClass>());

            // certificate
            rootCommand.AddCommand(AzmiCommandLineExtensions.ToCommand<GetCertificate, GetCertificate.AzmiArgumentsClass>());

            // return generated command
            return rootCommand;
        }
    }
}