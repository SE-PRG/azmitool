using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
                    new AzmiArgument("exclude", multiValued: true,
                        description: "Exclude blobs that match given regular expression."),
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

            Task<List<string>> task = ExecuteAsync(opt.container, opt.identity, opt.prefix, opt.exclude);
            List<string> results = task.Result;
            return results;
        }

        //
        // Execute ListBlobs
        //

        public async Task<List<string>> ExecuteAsync(Uri container, string identity = null, string prefix = null, string[] exclude = null)
        {
            var cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(container, cred);

            try
            {
                List<string> blobListing = new List<string>();
                await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    blobListing.Add(blob.Name);
                }

                if (exclude != null)
                { // apply --exclude regular expression
                    var rx = new Regex(String.Join('|',exclude));
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
