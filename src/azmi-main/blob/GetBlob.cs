using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public class GetBlob : BaseCommand
    {
        //
        // Declare command elements
        //

        public new string name = "getblob2";
        public string description = "test for classified getblob subcommand";

        public AzmiOption[] azmiOptions = new AzmiOption[] {
            SharedAzmiOptions.identity,
            SharedAzmiOptions.verbose,
            new AzmiOption("blobURL"),
            new AzmiOption("filePath"),
            new AzmiOption("ifNewer", null, AcceptedTypes.boolType ),
            new AzmiOption("deleteAfterCopy", AcceptedTypes.boolType)
        };

        public class Options : SharedOptions
        {
            public string blobURL;
            public string filePath;
            public bool ifNewer;
            public bool deleteAfterCopy;
        }

        //
        // Execute GetToken
        //

        public string Execute(Options options)
        {
            // parse arguments
            string identity = options.identity;
            string blobURL = options.blobURL;
            string filePath = options.filePath;
            bool ifNewer = options.ifNewer;
            bool deleteAfterCopy = options.deleteAfterCopy;

            // method start
            return $"id: {identity}, blob: {blobURL}, file: {filePath}";
        }
    }
}
