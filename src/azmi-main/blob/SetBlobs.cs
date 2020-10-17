using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;
using NLog;

namespace azmi_main
{
    public class SetBlobs : IAzmiCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(SetBlobs);

        private const char blobPathDelimiter = '/';

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

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
                    new AzmiArgument("skip-if-same", alias: null, type: ArgType.flag,
                        description: "Skip setting particular blob in batch if local file and already existing remote blob are same. Try to upload otherwise."),
                    new AzmiArgument("exclude", "Exclude blobs that match given regular expression."),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string directory { get; set; }
            public Uri container { get; set; }
            public string exclude { get; set; }
            public bool force { get; set; }
            public bool skipIfSame { get; set; }
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

            return Execute(opt.container, opt.directory, opt.identity, opt.exclude, opt.force, opt.skipIfSame);
        }


        //
        // SetBlobs main method
        //

        public List<string> Execute(Uri container, string directory, string identity = null, string exclude = null, bool force = false, bool skipIfSame = false)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            // authentication
            var cred = new ManagedIdentityCredential(identity);
            Uri containerTrimmed = new Uri(container.ToString().TrimEnd(blobPathDelimiter));
            var containerClient = new BlobContainerClient(containerTrimmed, cred);

            // get list of files to be uploaded
            string fullDirectoryPath = Path.GetFullPath(directory);
            var fileList = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories);

            // apply "--exclude" regular expression
            if (!String.IsNullOrEmpty(exclude)) {
                Regex excludeRegEx = new Regex(exclude);
                fileList = fileList.Where(file => !excludeRegEx.IsMatch(file));
            }

            // upload blobs
            List<string> results = new List<string>();
            Parallel.ForEach(fileList, file =>
            {
                var blobPath = file.Substring(fullDirectoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
                BlobClient blobClient = containerClient.GetBlobClient(blobPath);

                // "--skip-if-same" flag
                if (skipIfSame && blobClient.Exists())
                {
                    Response<BlobProperties> response = blobClient.GetProperties();
                    BlobProperties blobProperties = response.Value;
                    byte[] blobHash = blobProperties?.ContentHash;

                    if (blobHash != null)
                    {
                        byte[] fileHash = GetMD5HashFromFile(file);
                        if (fileHash.SequenceEqual(blobHash))
                        {
                            lock (results)
                            {
                                results.Add($"File '{file}' and blob '{blobClient.Uri}' are same. Skip setting the blob.");
                            }
                            // continue in Parallel.ForEach sequence
                            return;
                        }
                    }
                }

                try
                {
                    blobClient.Upload(file, force);

                    lock (results)
                    {
                        results.Add($"Success '{blobClient.Uri}'");
                    }
                }
                catch (Exception ex)
                {
                    throw AzmiException.IDCheck(identity, ex);
                }
            });

            return results;
        }

        private byte[] GetMD5HashFromFile(string filePath)
        {
            MD5 md5 = MD5.Create();
            FileStream stream = File.OpenRead(filePath);

            return md5.ComputeHash(stream);
        }
    }
}
