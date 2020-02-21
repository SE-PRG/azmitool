using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace azmi_main
{
    public static class Operations
    {
        // Class defining main operations performed by azmi tool

        private static Exception IdentityError(string identity, Exception ex)
        {
            // if no identity, then append identity missing error, otherwise just return existing exception
            if (string.IsNullOrEmpty(identity)) {
                return new ArgumentNullException("Missing identity argument", ex);
            } else if (ex.Message.Contains("See inner exception for details.") 
                && (ex.InnerException != null) 
                && (ex.InnerException.Message.Contains("Identity not found"))) {
                return new ArgumentException("Managed identity not found", ex);
            } else {
                return ex;
            }
        }

        public static string getToken(string endpoint = "management", string identity = null)
        {
            var Cred = new ManagedIdentityCredential(identity);
            if (string.IsNullOrEmpty(endpoint)) { endpoint = "management"; };
            var Scope = new String[] { $"https://{endpoint}.azure.com" };
            var Request = new TokenRequestContext(Scope);

            try
            {
                var Token = Cred.GetToken(Request);
                return Token.Token;
            } catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }

        // Download the blob to a local file
        public static string getBlob(string blobURL, string filePath, string identity = null, bool ifNewer = false)
        {
            // Connection
            var Cred = new ManagedIdentityCredential(identity);
            var blobClient = new BlobClient(new Uri(blobURL), Cred);

            if (ifNewer && File.Exists(filePath))
            {
                var blobProperties = blobClient.GetProperties();
                // Any operation that modifies a blob, including an update of the blob's metadata or properties, changes the last modified time of the blob
                var blobLastModified = blobProperties.Value.LastModified.UtcDateTime;

                // returns date of local file was last written to
                DateTime fileLastWrite = File.GetLastWriteTimeUtc(filePath);

                int value = DateTime.Compare(blobLastModified, fileLastWrite);
                if (value < 0)
                    return "Skipped. Blob is not newer than file.";
            }

            try
            {
                blobClient.DownloadTo(filePath);
                return "Success";
            }
            catch (Azure.RequestFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }

        public static List<string> listBlobs(string containerUri, string identity = null, string prefix = null)
        {
            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();

            try
            {
                List<string> blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();
                return blobListing.Count == 0 ? null : blobListing;
            }
            catch (Exception ex)
            {
                throw IdentityError(identity, ex);
           }
        }

        // sets blob content based on local file content into container
        public static string setBlob_byContainer(string filePath, string containerUri, bool force = false, string identity = null)
        {
            if (!(File.Exists(filePath))) {
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
                throw IdentityError(identity, ex);
            }
        }       

        public static string setBlob_byBlob(string filePath, string blobUri, bool force = false, string identity = null)
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
                throw IdentityError(identity, ex);
            }
        }
    }
}
