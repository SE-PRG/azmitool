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
                description = "test for classified listblobs subcommand",

                arguments = new AzmiOption[] {
                    new AzmiOption("container", required: true, type: ArgType.url,
                        description: "URL of container for which to list blobs. Example: https://myaccount.blob.core.windows.net/mycontainer"),
                    new AzmiOption("prefix",
                        description: "Specifies a string that filters the results to return only blobs whose name begins with the specified prefix"),
                    new AzmiOption("exclude",
                        description: "Exclude blobs that match given regular expression."),
                    SharedAzmiOptions.identity,
                    SharedAzmiOptions.verbose
                }
            };
        }

        public class Options : SharedOptions
        {
            public string container { get; set; }
            public string prefix { get; set; }
            public string exclude { get; set; }
        }

        public List<string> Execute(object options)
        {
            Options opt;
            try
            {
                opt = (Options)options;
            } catch
            {
                throw new Exception("Cannot convert object to proper class");
            }

            return Execute(opt.container, opt.identity, opt.prefix, opt.exclude);
        }

        //
        // Execute ListBlobs
        //


        public List<string> Execute(string containerUri, string identity = null, string prefix = null, string exclude = null)
        {
            // just for testing
            return $"cont={containerUri}, id={identity}".ToStringList();

            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();

            try
            {
                List<string> blobListing = new List<string>();
                if (exclude != null)
                { // apply --exclude regular expression
                    var rx = new Regex(exclude);
                    blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => rx.IsMatch(i.Name) ? null : i.Name).ToList();
                    while (blobListing.Remove(null)) { };
                } else
                { // return full list
                    blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();
                }

                return blobListing.Count == 0 ? null : blobListing;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

    }
}
