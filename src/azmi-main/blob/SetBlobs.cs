using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace azmi_main
{
    public class SetBlobs : IAzmiCommand
    {
        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "setblobs",
                description = "Writes multiple local files to storage account blobs.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("directory","Path to a local directory where local files are located. Examples: /home/avalanche/tmp/ or ./",
                        required: true),
                    new AzmiArgument("container","URL of container to which to upload files. Example: https://myaccount.blob.core.windows.net/mycontainer",
                        required: true),
                    new AzmiArgument("force", alias: null, type: ArgType.flag,
                        description: "Overwrite existing blob in Azure."),
                    new AzmiArgument("exclude", "Exclude blobs that match given regular expression."),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string directory { get; set; }
            public string container { get; set; }
            public string exclude { get; set; }
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

            return Execute(opt.container, opt.directory, opt.identity, opt.exclude, opt.force);
        }

        public List<string> Execute(string containerUri, string directory, string identity = null, string exclude = null, bool force = false)
        {
            List<string> results = new List<string>();
            string fullDirectoryPath = Path.GetFullPath(directory);
            int failures = 0;

            foreach (var file in Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories))
            {
                var blobUri = containerUri + file.Substring(fullDirectoryPath.Length);
                try
                {
                    string result = SetBlob.setBlob_byBlob(file, blobUri, identity, force);
                    results.Add(result + ' ' + blobUri);
                }
                catch
                {
                    results.Add("Failed " + blobUri);
                    failures++;
                }
            }
            results.Add(failures == 0 ? "Success" : $"Failed {failures} blobs");
            return results;
        }
    }
}
