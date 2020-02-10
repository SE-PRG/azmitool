using System;
using System.IO;

using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.Reflection.Metadata;

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

        public static string getBlob(string blobURL, string filePath, string identity = null)
        {
            // Download the blob to a local file
            var Cred = new ManagedIdentityCredential(identity);
            var blobClient = new BlobClient(new Uri(blobURL), Cred);
            try
            {
                blobClient.DownloadTo(filePath);
                return "Success";
            } catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }

        public static string setBlob(string filePath, string containerUri, string identity = null)
        {
            // sets blob content based on local file content
            if (!(File.Exists(filePath))) {
                throw new FileNotFoundException($"File '{filePath}' not found!");
            }

            var Cred = new ManagedIdentityCredential(identity);
            var containerClient = new BlobContainerClient(new Uri(containerUri), Cred);
            containerClient.CreateIfNotExists();
            var blobClient = containerClient.GetBlobClient(filePath.TrimStart('/'));
            try
            {
                blobClient.Upload(filePath);
                return "Success";
            } catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }
    }
}
