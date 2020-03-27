using System;
using System.IO;

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
                    new AzmiOption("blobURL", required: true,
                        description: "URL of blob which will be downloaded. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob"),
                    new AzmiOption("filePath", required: true,
                        description: "Path to local file to which content will be downloaded. Examples: /tmp/1.txt, ./1.xml"),
                    SharedAzmiOptions.identity,
                    new AzmiOption("ifNewer", alias: null, type: ArgType.flag,
                        description: "Download a blob only if a newer version exists in a container."),
                    new AzmiOption("deleteAfterCopy", type: ArgType.flag,
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
            // return $"id: {identity}, blob: {blobURL}, file: {filePath}";

            // Connection
            var Cred = new ManagedIdentityCredential(identity);
            var blobClient = new BlobClient(new Uri(blobURL), Cred);

            if (ifNewer && File.Exists(filePath))
            {
                var blobProperties = blobClient.GetProperties();
                // Any operation that modifies a blob, including an update of the blob's metadata or properties, changes the last modified time of the blob
                var blobLastModified = blobProperties.Value.LastModified.UtcDateTime;

                // returns date of local file was last written to
                DateTime fileLastWrite = File.GetLastWriteTimeUtc(filePath);

                int value = DateTime.Compare(blobLastModified, fileLastWrite);
                if (value < 0)
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
                throw ex;
                // TODO: Roll back this
            }

        }
    }
}
