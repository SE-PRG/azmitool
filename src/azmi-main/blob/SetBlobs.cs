﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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


        //
        // SetBlobs main method
        //

        public List<string> Execute(string containerUri, string directory, string identity = null, string exclude = null, bool force = false)
        {
            List<string> results = new List<string>();
            string fullDirectoryPath = Path.GetFullPath(directory);

            var fileList = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories);

            if (!String.IsNullOrEmpty(exclude)) {
                Regex excludeRegEx = new Regex(exclude);
                fileList = fileList.Where(file => !excludeRegEx.IsMatch(file));
            }

            foreach (var file in fileList)
            {
                var blobUri = containerUri + file.Substring(fullDirectoryPath.Length);
                string result = SetBlob.Execute(file, blobUri, identity, force);
                results.Add(result + ' ' + blobUri);
            }
            return results;
        }
    }
}