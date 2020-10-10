using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Identity;

namespace azmi_main
{
    public class GetBlob : IAzmiCommand
    {
        private IBlobClient blobClient { get; set; }

        //
        //  Constructors
        //

        public GetBlob() { }

        public GetBlob(IBlobClient blobClientMock)
        {
            blobClient = blobClientMock;
        }


        //
        //  Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "getblob",
                description = "Downloads blob from storage account to local file.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("blob", required: true, type: ArgType.url,
                        description: "URL of blob which will be downloaded. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob"),
                    new AzmiArgument("file", required: true,
                        description: "Path to local file to which content will be downloaded. Examples: /tmp/1.txt, ./1.xml"),
                    SharedAzmiArguments.identity,
                    new AzmiArgument("if-newer", alias: null, type: ArgType.flag,
                        description: "Download a blob only if a newer version exists in a container."),
                    new AzmiArgument("delete-after-copy", type: ArgType.flag,
                        description: "Successfully downloaded blob is removed from a container."),
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public Uri blob { get; set; }
            public string file { get; set; }
            public bool ifNewer { get; set; }
            public bool deleteAfterCopy { get; set; }
        }

        public List<string> Execute(object options) {
            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            Task<string> task = ExecuteAsync(opt.blob, opt.file, opt.identity, opt.ifNewer, opt.deleteAfterCopy);
            List<string> results = task.Result.ToStringList();
            return results;
        }

        //
        // Execute GetBlob
        //

        public async Task<string> ExecuteAsync(Uri blob, string filePath, string identity = null, bool ifNewer = false, bool deleteAfterCopy = false)
        {
            // method start

            // Connection
            var cred = new ManagedIdentityCredential(identity);
            blobClient ??= new BlobClientImpl(blob, cred);

            if (ifNewer && File.Exists(filePath) && !await IsNewer(blobClient, filePath))
            {
                return "Skipped. Blob is not newer than file.";
            }

            try
            {
                string absolutePath = Path.GetFullPath(filePath);
                string dirName = Path.GetDirectoryName(absolutePath);
                Directory.CreateDirectory(dirName);

                await blobClient.DownloadToAsync(filePath).ConfigureAwait(false);

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

        private async Task<bool> IsNewer(IBlobClient blob, string filePath)
        {
            var blobProperties = await blob.GetPropertiesAsync().ConfigureAwait(false);
            // Any operation that modifies a blob, including an update of the blob's metadata or properties, changes the last modified time of the blob
            var blobLastModified = blobProperties.Value.LastModified.UtcDateTime;

            // returns date of local file was last written to
            DateTime fileLastWrite = File.GetLastWriteTimeUtc(filePath);

            return blobLastModified > fileLastWrite;
        }
    }
}
