using azmi_main;
using System;

namespace azmi_commandline
{
    class Program
    {
        static void Main(string[] args)
        {
            if (String.IsNullOrEmpty(args[0]))
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
            else if (String.IsNullOrEmpty(args[1]) && args[1] == "help") {
                HelpMessage.command(args[0]);
            }
            else if (args[0] == "setblob") {
                
                //
                // set blob command
                //
                
                if (String.IsNullOrEmpty(args[1]) || String.IsNullOrEmpty(args[2]))
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
