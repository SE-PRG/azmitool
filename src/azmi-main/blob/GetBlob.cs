using System;
using System.IO;
using System.Collections.Generic;

using Azure.Identity;
using Azure.Storage.Blobs;


namespace azmi_main
{
    public class GetBlob : IAzmiCommand
    {
        //
        // Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "getblob",
                description = "test for classified getblob subcommand",

                arguments = new AzmiOption[] {
                    new AzmiOption("blobURL", required: true, type: ArgType.url,
                        description: "URL of blob which will be downloaded. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob"),
                    new AzmiOption("filePath", required: true,
                        description: "Path to local file to which content will be downloaded. Examples: /tmp/1.txt, ./1.xml"),
                    SharedAzmiOptions.identity,
                    new AzmiOption("if-newer", alias: null, type: ArgType.flag,
                        description: "Download a blob only if a newer version exists in a container."),
                    new AzmiOption("delete-after-copy", type: ArgType.flag,
                        description: "Successfully downloaded blob is removed from a container."),
                    SharedAzmiOptions.verbose
                }
            };
        }

        public class Options : SharedOptions
        {
            public string blobURL { get; set; }
            public string filePath { get; set; }
            public bool ifNewer { get; set; }
            public bool deleteAfterCopy { get; set; }
        }

        public List<string> Execute(object options) {
            Options opt;
            try
            {
                opt = (Options)options;
            } catch
            {
                throw new ArgumentException("Cannot convert object to proper class");
            }

            return Execute(opt.blobURL, opt.filePath, opt.identity, opt.ifNewer, opt.deleteAfterCopy).ToStringList();
        }

        //
        // Execute GetBlob
        //

        public string Execute(string blobURL, string filePath, string identity = null, bool ifNewer = false, bool deleteAfterCopy = false)
        {

            // method start
            return $"id: {identity}, blob: {blobURL}, file: {filePath}";


            // Connection
            var Cred = new ManagedIdentityCredential(identity);
            var blobClient = new BlobClient(new Uri(blobURL), Cred);

            if (ifNewer && File.Exists(filePath) && IsNewer(blobClient, filePath))
            {
                return "Skipped. Blob is not newer than file.";
            }

            try
            {
                string absolutePath = Path.GetFullPath(filePath);
                string dirName = Path.GetDirectoryName(absolutePath);
                Directory.CreateDirectory(dirName);

                blobClient.DownloadTo(filePath);

                if (deleteAfterCopy)
                {
                    blobClient.Delete();
                }
                return "Success";

            } catch (Azure.RequestFailedException)
            {
                throw;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
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
