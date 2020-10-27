using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using System.IO.Abstractions;

namespace azmi_main
{
    public class GetBlob : IAzmiCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(GetBlob);

        private IBlobClient blobClient { get; set; }
        private readonly IFileSystem fileSystem;

        //
        //  Constructors
        //

        public GetBlob() {
            this.fileSystem = new FileSystem();
        }

        public GetBlob(IBlobClient blobClientMock)
        {
            blobClient = blobClientMock;
            this.fileSystem = new FileSystem();
        }

        public GetBlob(IBlobClient blobClientMock, IFileSystem fileSystem)
        {
            blobClient = blobClientMock;
            this.fileSystem = fileSystem;
        }

        //
        //  Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

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
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.blob, opt.file, opt.identity, opt.ifNewer, opt.deleteAfterCopy).ToStringList();
        }

        //
        // Execute GetBlob
        //

        public string Execute(Uri blob, string filePath, string identity = null, bool ifNewer = false, bool deleteAfterCopy = false)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            // Connection
            var cred = new ManagedIdentityCredential(identity);
            blobClient ??= new BlobClientImpl(blob, cred);

            if (ifNewer && fileSystem.File.Exists(filePath) && !IsNewer(blobClient, filePath))
            {
                return "Skipped. Blob is not newer than file.";
            }

            try
            {
                string absolutePath = Path.GetFullPath(filePath);
                string dirName = Path.GetDirectoryName(absolutePath);
                fileSystem.Directory.CreateDirectory(dirName);

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

        private bool IsNewer(IBlobClient blob, string filePath)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            var blobProperties = blob.GetProperties();
            // Any operation that modifies a blob, including an update of the blob's metadata or properties, changes the last modified time of the blob
            var blobLastModified = blobProperties.Value.LastModified.UtcDateTime;

            // returns date of local file was last written to
            DateTime fileLastWrite = fileSystem.File.GetLastWriteTimeUtc(filePath);

            return blobLastModified > fileLastWrite;
        }
    }
}
