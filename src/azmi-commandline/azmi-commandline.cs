using azmi_main;
using System;
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
                Environment.Exit(0);
            }
            else if (args.Length == 2 && args[1] == "help")
            {
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
            else if (args[0] == "getblob")
            {
                //
                // get blob subcommand
                // azmi getblob $BLOB $FILE
                //
                if (args.Length != 3)
                {
                    // required parameters error, display setblob usage
                    WriteLines(HelpMessage.subCommand("getblob"));
                    Environment.Exit(1);
                }
                else
                {
                    try
                    {
                        // call getblob method
                        WriteLines(Operations.getBlob(args[1], args[2]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General Error: {0}", ex.Message);
                    }
                }
                // end of getblob command
            }
            else if (args[0] == "setblob")
            {
                //
                // set blob subcommand
                // azmi setblob $FILE $BLOB
                //
                if (args.Length != 3)
                {
                    // required parameters error, display setblob usage                    
                    WriteLines(HelpMessage.subCommand("setblob"));
                    Environment.Exit(1);
                }
                else
                {
                    try
                    {
                        // call setblob method
                        WriteLines(Operations.setBlob(args[1], args[2]));
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General Error: {0}", ex.Message);
                    }
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
                    try
                    {
                        // returns token obtained from default endpoint
                        WriteLines(Operations.getToken());
                    }                   
                    catch (Exception ex)
                    {
                        Console.WriteLine("General error: {0}", ex.Message);
                    }
                }
                else if (args.Length == 2)
                { // we have already covered option azmi gettoken help
                    try
                    {
                        WriteLines(Operations.getToken(args[1]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General Error: {0}", ex.Message);
                    }
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
                WriteLines("Error: Unrecognized command(s).");
                WriteLines(HelpMessage.application());                
                Environment.Exit(1);
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
