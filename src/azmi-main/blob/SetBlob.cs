using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;

namespace azmi_main
{
    public class SetBlob : IAzmiCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(SetBlob);

        private IBlobClient blobClient { get; set; }

        //
        //  Constructors
        //

        public SetBlob() { }

        public SetBlob(IBlobClient blobClientMock)
        {
            blobClient = blobClientMock;
        }


        //
        //  Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            return new SubCommandDefinition
            {

                name = "setblob",
                description = "Writes local file to storage account blob.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("file", required: true,
                        description: "Path to local file which will be uploaded. Examples: /tmp/1.txt, ./1.xml"),
                    new AzmiArgument("blob", required: true, type: ArgType.url,
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
            public Uri blob { get; set; }
            public bool force { get; set; }
        }

        public List<string> Execute(object options)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            }
            catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.file, opt.blob, opt.identity, opt.force).ToStringList();
        }

        //
        // Execute SetBlob
        //

        public string Execute(string filePath, Uri blob, string identity = null, bool force = false)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            var Cred = new ManagedIdentityCredential(identity);
            blobClient ??= new BlobClientImpl(blob, Cred);

            try
            {
                blobClient.Upload(filePath, force);
                return "Success";
            }
            catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }
    }
}
