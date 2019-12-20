using azmi_main;
using System;
using System.Security.Cryptography;

namespace azmi_commandline
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // display usage and error
                WriteLines(HelpMessage.application());
                Environment.Exit(1);

            }
            else if (args[0] == "help")
            {
                // display usage
                WriteLines(HelpMessage.application());
                //Environment.Exit(0);
            }
            else if (args.Length == 2 && args[1] == "help") {
                // Command specific help. like "azmi 0:setblob 1:help"
                WriteLines(HelpMessage.subCommand(args[0]));
                // TODO: Verify if args[0] is defined function, using predefined list in main project
            }
            else if (args[0] == "setblob") {

                //
                // set blob subcommand
                // azmi setblob $BLOB $FILE
                //

                if (args.Length != 3)
                {
                    // required parameters error, display setblob usage                    
                    WriteLines(HelpMessage.subCommand("setblob"));
                    Environment.Exit(1);
                }
                else
                {
                    // call setblob method
                    WriteLines(Operations.setBlob(args[1], args[2]));

                }
                // end of setblob command
            }
            else if (args[0] == "gettoken")
            {

                //
                // get token subcommand
                // azmi gettoken [$ENDPOINT]
                //

                if (args.Length == 1)
                {
                    // returns token obtained from default endpoint
                    WriteLines(Operations.getToken());
                }
                else if (args.Length == 2)
                {
                    WriteLines(Operations.getToken(args[1]));
                    // we have already covered option azmi gettoken help
                }
                else if (args.Length != 2)
                {
                    // parameters error, display setblob usage                    
                    WriteLines(HelpMessage.subCommand("gettoken"));
                    Environment.Exit(1);
                }

            }
            //
            // add here additional commands
            //
            else
            {
                // error unrecognized command
                HelpMessage.application();
                throw new ArgumentException();
            }
        }

        private static void WriteLines(string[] s)
        {
            foreach (var s1 in s) { Console.WriteLine(s1); };
        }

        private static void WriteLines(string s)
        {
            Console.WriteLine(s);
        }
    }
}
