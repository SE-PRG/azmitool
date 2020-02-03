using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public static class HelpMessage
    {

        public static string[] supportedSubCommands = { "gettoken", "getblob", "setblob" };
        // TODO: Use definition above both in command line and tests projects
        // TODO: Add definition of command arguments (i.e. count, names, description)


        //
        // Main help message for the application
        //

        public static string[] application()
        {
            var response = new List<string>() { @"
Command-line utility azmi stands for Azure Managed Identity.
It is helping admins simplify common operations (reading / writing) on standard Azure resources.
It is utilizing Azure AD authentication via user assigned managed identity.

Usage:
  azmi help - displays this help message" };
            foreach (var subCommand in supportedSubCommands)
            {
                response.Add($"  azmi {subCommand} help - displays help on {subCommand} sub-command");
            }
            return response.ToArray();
        }


        //
        // Individual sub-commands help messages
        //

        public static string[] subCommand(string commandName)
        {
            if (commandName == "getblob")
            {
                return new string[] { @"
Subcommand 'getblob' is used for reading contents from storage account blob.

Usage:
azmi getblob help - displays this help message
azmi getblob $URL $FILE - reads contents from storage account blob (URL) and writes to a local file (FILE)" };
            } else if (commandName == "setblob")
            {
                return new string[] { @"
Subcommand 'setblob' is used for writing to storage account blob.

Usage:
azmi setblob help - displays this help message
azmi setblob $FILE $CONTAINER - writes a local file (FILE) to a storage account container (CONTAINER)" };
            } else if (commandName == "gettoken")
            {
                return new string[] { @"
Subcommand 'gettoken' is used for obtaining Azure authorization token.

Usage:
azmi gettoken help - displays this help message              
azmi gettoken [$ENDPOINT] obtains token against management (default value) or storage endpoints" };
            } else
            {
                throw new ArgumentNullException($"Unknown help for subcommand '{commandName}'.");
            }
        }
    }
}
