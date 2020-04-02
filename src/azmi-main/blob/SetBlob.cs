using System;
using System.Collections.Generic;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.IO;

namespace azmi_main
{
    public class SetBlob : IAzmiCommand
    {
        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "setblob",
                description = "Writes local file to storage account blob.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("file", required: true,
                        description: "Path to local file which will be uploaded. Examples: /tmp/1.txt, ./1.xml"),
                    new AzmiArgument("container", required: false, type: ArgType.url,
                        description: "URL of container to which file will be uploaded. Cannot be used together with --blob. Example: https://myaccount.blob.core.windows.net/mycontainer"),
                    new AzmiArgument("blob", required: false, type: ArgType.url,
                        description: "URL of blob to which file will be uploaded. Cannot be used together with --container. Example: https://myaccount.blob.core.windows.net/mycontainer/myblob.txt"),
                    new AzmiArgument("force", alias: null, type: ArgType.flag,
                        description: "Overwrite existing blob in Azure."),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string file { get; set; }
            public string container { get; set; }
            public string blob { get; set; }
            public bool force { get; set; }
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

            if (String.IsNullOrEmpty(opt.blob) && String.IsNullOrEmpty(opt.container))
            {
                // TODO: Setup AzmiExceptions
                throw new ArgumentException("You must specify either blob or container url");
            } else if ((!String.IsNullOrEmpty(opt.blob)) && (!String.IsNullOrEmpty(opt.container)))
            {
                // TODO: Setup AzmiExceptions
                throw new ArgumentException("Cannot use both container and blob url");
            }

            if (String.IsNullOrEmpty(opt.blob))
            {
                return SetBlob.setBlob_byContainer(opt.file, opt.container, opt.identity, opt.force).ToStringList();
            } else
            {
                return SetBlob.setBlob_byBlob(opt.file, opt.blob, opt.identity, opt.force).ToStringList();
            }
        }

        //
        // Execute SetBlob
        //

        public static string setBlob_byContainer(string filePath, string containerUri, string identity = null, bool force = false)
        {
            if (!(File.Exists(filePath)))
            {
                throw new FileNotFoundException($"File '{filePath}' not found!");
            }

            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();
            var blobClient = containerClient.GetBlobClient(filePath.TrimStart('/'));
            try
            {
                blobClient.Upload(filePath, force);
                return "Success";
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

        // Sets blob content based on local file content into blob
        public static string setBlob_byBlob(string filePath, string blobUri, string identity = null, bool force = false)
        {
            // sets blob content based on local file content with provided blob url
            if (!(File.Exists(filePath)))
            {
                throw new FileNotFoundException($"File '{filePath}' not found!");
            }

            var Cred = new ManagedIdentityCredential(identity);
            var blobClient = new BlobClient(new Uri(blobUri), Cred);
            try
            {
                blobClient.Upload(filePath, force);
                return "Success";
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }


    }
}
