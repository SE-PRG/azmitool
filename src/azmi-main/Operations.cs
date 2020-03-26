using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace azmi_main
{
    // Class defining main operations performed by azmi tool
    public class Operations : IOperations
    {
        // Constructor
        public Operations() { }
        // Destructor
        ~Operations() { }

        private Exception IdentityError(string identity, Exception ex)
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

        // Get MSI access token
        public string getToken(string endpoint = "management", string identity = null, bool JWTformat = false)
        {
            var Cred = new ManagedIdentityCredential(identity);
            if (string.IsNullOrEmpty(endpoint)) { endpoint = "management"; };
            var Scope = new String[] { $"https://{endpoint}.azure.com" };
            var Request = new TokenRequestContext(Scope);

            try
            {
                var Token = Cred.GetToken(Request);

                if (JWTformat)
                {
                    var stream = Token.Token;                    
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(stream);
                    var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
                    return tokenS.ToString(); // decoded JSON Web Token
                }
                else
                {
                    return Token.Token; // encoded JWT token
                }
            } catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }

        // Download the blob to a local file
        public string getBlob(string blobURL, string filePath, string identity = null, bool ifNewer = false, bool deleteAfterCopy = false)
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
                string absolutePath = Path.GetFullPath(filePath);
                string dirName = Path.GetDirectoryName(absolutePath);
                Directory.CreateDirectory(dirName);

                blobClient.DownloadTo(filePath);

                if (deleteAfterCopy)
                {
                    blobClient.Delete();
                }
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

        // Download blobs from container to directory
        public List<string> getBlobs(string containerUri, string directory, string identity = null, string prefix = null, string exclude = null, bool ifNewer = false, bool deleteAfterCopy = false)
        {
            string containerUriTrimmed = containerUri.TrimEnd('/');
            List<string> blobsListing = this.listBlobs(containerUriTrimmed, identity, prefix, exclude);
            if (blobsListing == null)
                return null;

            List<string> results = new List<string>();
            string result = null;
            int failures = 0;
            foreach (var blob in blobsListing)
            {
                // e.g. blobUri = https://<storageAccount>.blob.core.windows.net/Hello/World.txt
                string blobUri = containerUriTrimmed + '/' + blob;
                string filePath = directory + '/' + blob;
                try
                {
                    result = this.getBlob(blobUri, filePath, identity, ifNewer, deleteAfterCopy);
                    string downloadStatus = result + ' ' + blob;
                    results.Add(downloadStatus);
                } catch
                {
                    results.Add("Failed " + blob);
                    failures++;
                }
            }
            results.Add(failures == 0 ? "Success" : $"Failed {failures} blobs");
            return results;
        }

        // List blobs in a container
        public List<string> listBlobs(string containerUri, string identity = null, string prefix = null, string exclude = null)
        {
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
                }
                else
                { // return full list
                    blobListing = containerClient.GetBlobs(prefix: prefix).Select(i => i.Name).ToList();
                }

                return blobListing.Count == 0 ? null : blobListing;
            }
            catch (Exception ex)
            {
                throw IdentityError(identity, ex);
           }
        }

        // Sets blob content based on local file content into container
        public string setBlob_byContainer(string filePath, string containerUri, string identity = null, bool force = false)
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

        // Sets blob content based on local file content into blob
        public string setBlob_byBlob(string filePath, string blobUri, string identity = null, bool force = false)
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

        // Validate URL to a key vault secret is generally valid
        private (Uri, string) validateAndParseSecretURL(string secretIdentifierUrl)
        {
            // Example of expected URL: https://my-key-vault.vault.azure.net/secrets/mySecret.pwd
            if (!Uri.IsWellFormedUriString(secretIdentifierUrl, UriKind.Absolute))
                throw new UriFormatException($"Provided URL '{secretIdentifierUrl}' is not well formed URL.");

            Uri secretIdentifierUri = new Uri(secretIdentifierUrl);

            if (secretIdentifierUri.Scheme != Uri.UriSchemeHttps)
                throw new UriFormatException($"Only '{Uri.UriSchemeHttps}' protocol is supported.");

            // e.g. http://my-key-vault.vault.azure.net
            Uri keyVaultUri = new Uri(secretIdentifierUri.GetLeftPart(UriPartial.Authority));
            // Segments = /, secrets/, mySecret.pwd
            if (secretIdentifierUri.Segments.Count() <= 2)
                throw new UriFormatException($"URL '{secretIdentifierUrl}' is missing a path to secret.");

            string secretName = secretIdentifierUri.Segments.Last();

            return (keyVaultUri, secretName);
        }

        // Get secret from key vault
        public string getSecret(string secretIdentifierUrl, string identity = null)
        {
            (Uri keyVaultUri, string secretName) = this.validateAndParseSecretURL(secretIdentifierUrl);

            var MIcredential = new ManagedIdentityCredential(identity);
            var secretClient = new SecretClient(keyVaultUri, MIcredential);

            // Retrieve a secret
            try
            {
                KeyVaultSecret secret = secretClient.GetSecret(secretName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw IdentityError(identity, ex);
            }
        }
    }
}
