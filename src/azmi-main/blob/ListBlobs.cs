using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Azure.Identity;
using Azure.Storage.Blobs;

namespace azmi_main
{
    public class ListBlobs : IAzmiCommand
    {
        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "listblobs",
                description = "List all blobs in container and send to output.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("container", required: true, type: ArgType.url,
                        description: "URL of container for which to list blobs. Example: https://myaccount.blob.core.windows.net/mycontainer"),
                    new AzmiArgument("prefix",
                        description: "Specifies a string that filters the results to return only blobs whose name begins with the specified prefix"),
                    new AzmiArgument("exclude",
                        description: "Exclude blobs that match given regular expression."),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string container { get; set; }
            public string prefix { get; set; }
            public string exclude { get; set; }
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

            return Execute(opt.container, opt.identity, opt.prefix, opt.exclude);
        }

        //
        // Execute ListBlobs
        //


        public List<string> Execute(string containerUri, string identity = null, string prefix = null, string exclude = null)
        {

            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();

            try
            {
                List<string> blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();

                if (exclude != null)
                { // apply --exclude regular expression
                    var rx = new Regex(exclude);
                    blobListing = blobListing.Where(b => !rx.IsMatch(b)).ToList();
                }
                return blobListing.Count == 0 ? null : blobListing;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

    }
}
