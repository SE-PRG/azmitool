using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using NLog;

namespace azmi_main
{
    public class ListBlobs : IAzmiCommand
    {

        private IContainerClient containerClient { get; set; }
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(ListBlobs);

        //
        //  Constructors
        //

        public ListBlobs() { }

        public ListBlobs(IContainerClient containerClientMock)
        {
            containerClient = containerClientMock;
        }


        //
        //  Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            return new SubCommandDefinition
            {

                name = "listblobs",
                description = "List all blobs in container and send to output.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("container", required: true, type: ArgType.url,
                        description: "URL of container for which to list blobs. Example: https://myaccount.blob.core.windows.net/mycontainer"),
                    new AzmiArgument("prefix",
                        description: "Specifies a string that filters the results to return only blobs whose name begins with the specified prefix"),
                    new AzmiArgument("exclude", multiValued: true,
                        description: "Exclude blobs that match given regular expression"),
                    new AzmiArgument("absolute-paths", type: ArgType.flag,
                        description: "List absolute paths (URLs) of blobs instead of relative ones"),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public Uri container { get; set; }
            public string prefix { get; set; }
            public string[] exclude { get; set; }
            public bool absolutePaths { get; set; }
        }

        public List<string> Execute(object options)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.container, opt.identity, opt.prefix, opt.exclude, opt.absolutePaths);
        }

        //
        // Execute ListBlobs
        //


        public List<string> Execute(Uri container, string identity = null, string prefix = null, string[] exclude = null, bool absolutePaths = false)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            var Cred = new ManagedIdentityCredential(identity);
            containerClient ??= new ContainerClientImpl(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();

            try
            {
                List<string> blobsListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();

                if (exclude != null)
                { // apply --exclude regular expression
                    var rx = new Regex(String.Join('|', exclude));
                    blobsListing = blobsListing.Where(b => !rx.IsMatch(b)).ToList();
                }

                if (absolutePaths)
                { // apply --absolute-path
                    List<string> blobsUris = new List<string>();
                    foreach (string blobName in blobsListing)
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(blobName);
                        blobsUris.Add(blobClient.Uri.ToString());
                    }
                    blobsListing = blobsUris.ToList();
                }

                return blobsListing.Count == 0 ? null : blobsListing;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

    }
}
