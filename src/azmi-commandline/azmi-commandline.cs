using azmi_main;
using System;
using System.Security.Cryptography;
using System.Linq;

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
                string invokeSubCommand = args[0];
                if (HelpMessage.supportedSubCommands.Contains(invokeSubCommand))
                {
                    // Command specific help. like "azmi 0:setblob 1:help"
                    WriteLines(HelpMessage.subCommand(invokeSubCommand));
                }
                else
                {
                    WriteLines($"Unrecognized subcommand '{invokeSubCommand}'.");
                    // display usage and error
                    WriteLines(HelpMessage.application());
                    Environment.Exit(1);
                }
            }
            else if (args[0] == "setblob") {
                
                //
                // set blob command
                // azmi setblob $BLOB $FILE
                //
                
                if (args.Length != 3)
                {
                    // requires parameters error, display setblob usage                    
                    WriteLines(HelpMessage.subCommand("setblob"));
                    Environment.Exit(1);
                }
                else
                {
                    // call setblob method
                    Console.WriteLine("Starting something");
                    WriteLines(Operations.setBlob(args[1], args[2]));

                }
                // end of setblob command
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
