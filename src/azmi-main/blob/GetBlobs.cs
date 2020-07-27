using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azmi_main
{
    public class GetBlobs : IAzmiCommand
    {
        private const char blobPathDelimiter = '/';

        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "getblobs",
                description = "Downloads blobs from container to local directory.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("container","URL of container blobs will be downloaded from. Example: https://myaccount.blob.core.windows.net/mycontainer",
                        required: true),
                    new AzmiArgument("directory","Path to a local directory to which blobs will be downloaded to. Examples: /home/avalanche/tmp/ or ./",
                        required: true),
                    new AzmiArgument("prefix", "Specifies a string that filters the results to return only blobs whose name begins with the specified prefix"),
                    new AzmiArgument("exclude", "Exclude blobs that match given regular expression."),
                    new AzmiArgument("if-newer", null, "Download blobs only if newer versions exist in a container.",
                        ArgType.flag),
                    new AzmiArgument("delete-after-copy", null, "Successfully downloaded blobs are removed from a container.",
                        ArgType.flag),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string container { get; set; }
            public string directory { get; set; }
            public string prefix { get; set; }
            public string exclude { get; set; }
            public bool ifNewer { get; set; }
            public bool deleteAfterCopy { get; set; }
        }

        public List<string> Execute(object options)
        {
            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }


            return Execute(opt.container, opt.directory, opt.identity, opt.prefix, opt.exclude, opt.ifNewer, opt.deleteAfterCopy);
        }


        //
        // GetBlobs main method
        //

        public List<string> Execute(string containerUri, string directory, string identity = null, string prefix = null, string exclude = null, bool ifNewer = false, bool deleteAfterCopy = false)
        {
            string containerUriTrimmed = containerUri.TrimEnd(blobPathDelimiter);
            List<string> blobsListing = new ListBlobs().Execute(containerUriTrimmed, identity, prefix, exclude);
            List<string> results = new List<string>();

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 2
            };
            Parallel.ForEach(blobsListing, options, blob =>
            // foreach (var blob in blobsListing)
            {
                // e.g. blobUri = https://<storageAccount>.blob.core.windows.net/Hello/World.txt
                string blobUri = containerUriTrimmed + blobPathDelimiter + blob;
                string filePath = Path.Combine(directory, blob);
                string result = new GetBlob().Execute(blobUri, filePath, identity, ifNewer, deleteAfterCopy);
                string downloadStatus = result + ' ' + blobUri;
                lock (results)
                {
                    results.Add(downloadStatus);
                }
            });
            return results;
        }
    }
}
