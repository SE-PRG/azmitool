using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public class GetBlob : IAzmiCommand
    {
        //
        // Declare command elements
        //

        public string Name() { return "getblob2"; }
        public string Description() { return "test for classified getblob subcommand"; }

        public AzmiOption[] AzmiOptions()
        {
            return new AzmiOption[] {
                SharedAzmiOptions.identity,
                SharedAzmiOptions.verbose,
                new AzmiOption("blobURL", required: true),
                new AzmiOption("filePath", required: true),
                new AzmiOption("ifNewer", null, AcceptedTypes.boolType ),
                new AzmiOption("deleteAfterCopy", AcceptedTypes.boolType)
            };
        }

        public class Options : SharedOptions
        {
            public string blobURL { get; set; }
            public string filePath { get; set; }
            public bool ifNewer { get; set; }
            public bool deleteAfterCopy { get; set; }
        }

        //
        // Execute GetToken
        //

        public string Execute(object options) { return Execute((Options)options); }

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
