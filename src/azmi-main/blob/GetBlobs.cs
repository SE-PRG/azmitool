using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Linq;
using System.Text.RegularExpressions;

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
            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUriTrimmed), Cred);

            List<string> blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();
            if (exclude != null)
            { // apply --exclude regular expression
                var rx = new Regex(exclude);
                blobListing = blobListing.Where(b => !rx.IsMatch(b)).ToList();
            }
            var results = new List<string>();
            Parallel.ForEach(blobListing, blobItem =>
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem);

                string filePath = Path.Combine(directory, blobItem);
                if (ifNewer && File.Exists(filePath) && !IsNewer(blobClient, filePath))
                {
                    lock (results)
                    {
                        results.Add("Skipped. Blob is not newer than file.");
                    }
                }

                string absolutePath = Path.GetFullPath(filePath);
                string dirName = Path.GetDirectoryName(absolutePath);
                Directory.CreateDirectory(dirName);

                try
                {
                    blobClient.DownloadTo(filePath);

                    lock (results)
                    {
                        results.Add("Success");
                    }

                    if (deleteAfterCopy)
                    {
                        blobClient.Delete();
                    }
                }
                catch (Azure.RequestFailedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw AzmiException.IDCheck(identity, ex);
                }
            });
            return results;
        }

        private bool IsNewer(BlobClient blob, string filePath)
        {
            var blobProperties = blob.GetProperties();
            // Any operation that modifies a blob, including an update of the blob's metadata or properties, changes the last modified time of the blob
            var blobLastModified = blobProperties.Value.LastModified.UtcDateTime;

            // returns date of local file was last written to
            DateTime fileLastWrite = File.GetLastWriteTimeUtc(filePath);

            return blobLastModified > fileLastWrite;
        }
    }
}
