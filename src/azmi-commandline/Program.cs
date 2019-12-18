using azmi_main;
using System;

namespace azmi_commandline
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // display usage and error
                HelpMessage.application();
                throw new ArgumentNullException();

            }
            else if (args[0] == "help")
            {
                // display usage
                HelpMessage.application();
            }
            else if (args.Length == 2 && args[1] == "help") {
                // Command specific help. like "azmi setblob help"
                HelpMessage.command(args[0]);
                // TODO: Verify if args[0] is defined function
            }
            else if (args[0] == "setblob") {
                
                //
                // set blob command
                // azmi setblob $BLOB $FILE
                //
                
                if (args.Length != 3)
                {
                    // requires parameters error, display setblob usage                    
                    HelpMessage.command("setblob");
                    throw new ArgumentNullException();
                }
                else
                {
                    // call setblob method
                    Operations.setBlob(args[1], args[2]);

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
    }
}
