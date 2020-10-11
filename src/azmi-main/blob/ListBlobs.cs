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
            var cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(container, cred);
            containerClient.CreateIfNotExists();

            List<string> results = new List<string>();
            try
            {
                List<string> blobsNames = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();

                List<string> blobsNamesExclusionApplied = new List<string>();
                if (exclude != null)
                { // apply --exclude regular expression
                    var rx = new Regex(String.Join('|',exclude));
                    blobsNamesExclusionApplied = blobsNames.Where(b => !rx.IsMatch(b)).ToList();
                    results = blobsNamesExclusionApplied;
                }

                if (absolutePaths)
                { // apply --absolute-path
                    List<string> blobsUris = new List<string>();
                    foreach (string blobName in blobsNamesExclusionApplied)
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(blobName);
                        blobsUris.Add(blobClient.Uri.ToString());
                    }
                    results = blobsUris;
                }

                return results.Count == 0 ? null : results;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

    }
}
