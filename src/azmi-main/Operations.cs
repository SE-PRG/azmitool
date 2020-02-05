using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;

using Azure.Storage.Blobs;
using Azure.Identity;


namespace azmi_main
{
    public static class Operations
    {
        // Class defining main operations performed by azmi tool

        public static string metadataUri(string endpoint = "management", string apiVersion = "2018-02-01")
        {
            string[] validEndpoints = { "management", "storage" };
            if (!(validEndpoints.Contains(endpoint)))
            {
                throw new ArgumentOutOfRangeException($"Metadata endpoint '{endpoint}' not supported.\n");
            }

            string uri = "http://169.254.169.254/metadata/identity/oauth2/token";
            uri += $"?api-version={apiVersion}";
            uri += $"&resource=https://{endpoint}.azure.com";

            return uri;
        }

        public static string getMetaDataResponse(string endpointUri = "")
        {
            // TODO: Extend this to support also provided managed identity name            
            // Build request to acquire managed identities for Azure resources token
            if (string.IsNullOrEmpty(endpointUri))
            {
                endpointUri = Operations.metadataUri();
            }

            var request = (HttpWebRequest)WebRequest.Create(endpointUri);
            request.Headers["Metadata"] = "true";
            request.Method = "GET";

            // TODO: Switch to HttpClient
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest?view=netframework-4.8#remarks
            //HttpClient client = new HttpClient();
            //HttpResponseMessage response2 = client.GetAsync("http://www.contoso.com/").Result;
            //response2.EnsureSuccessStatusCode();
            //string responseBody = response2.Content.ReadAsStringAsync().Result;

            try
            {
                // Call /token endpoint
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                var metaDataResponse = streamResponse.ReadToEnd();
                if (String.IsNullOrEmpty(metaDataResponse))
                {
                    throw new ArgumentNullException("Received empty response from metaData service.\n");
                } else
                {
                    return metaDataResponse;
                }
            } catch (Exception ex)
            {
                throw new Exception("Failed to receive response from metadata.\n" + ex.Message, ex);
            }
        }

        public static string extractToken(string metaDataResponse)
        {
            try
            {
                var obj = (Dictionary<string, string>)JsonSerializer.Deserialize(metaDataResponse, typeof(Dictionary<string, string>));
                return obj["access_token"];
            } catch (Exception ex)
            {
                throw new Exception("Could not deserialize access token.\n" + ex.Message, ex);
            }
        }

        public static string getToken(string endpoint = "")
        {
            // Method unifies above two mentioned methods into one
            return extractToken(getMetaDataResponse(endpoint));
        }

        public static string getBlob(string blobURL, string filePath)
        {
            // Blob naming
            // https://azmitest.blob.core.windows.net/azmi-test/tmp/azmi_integration_test_2020-01-29_04:34:16.txt
            //                    CONTAINER                    |                                                 |
            //                                                 |                      BLOB                       |

            // Download the blob to a local file
            BlobClient blobClient = null;
            try
            {                
                blobClient = new BlobClient(new Uri(blobURL), new ManagedIdentityCredential());
            } catch (Exception ex)
            {                
                throw new Exception("Can not setup blob client instance.\n" + ex.Message, ex);
            }

            Console.WriteLine("\nDownloading blob to:\n\t{0}\n", filePath);

            Azure.Storage.Blobs.Models.BlobDownloadInfo download;
            try
            {
                // Download the blob's contents
                download = blobClient.Download();
            } catch (Azure.RequestFailedException ex)
            {                
                throw new Exception("Download failed.\n" + ex.Message, ex);                
            }

            FileStream downloadFileStream = null;            
            try {
                // and save it to a file                
                downloadFileStream = File.OpenWrite(filePath);
                download.Content.CopyTo(downloadFileStream);
                downloadFileStream.Close();
                return "Success";
            }
            catch (Exception ex)
            {                
                throw new Exception("Saving file failed.\n" + ex.Message, ex);                              
            } finally
            {
                if (downloadFileStream != null)
                {
                    downloadFileStream.Close();
                }
            }            
        }

        public static string setBlob(string filePath, string containerUri)
        {
            // sets blob content based on local file content
            if (!(File.Exists(filePath)))
            {
                throw new FileNotFoundException($"File '{filePath}' not found!");
            }

            // TODO: Check if container uri contains blob path also, like container/folder1/folder2
            // Get a credential and create a client object for the blob container.            
            BlobContainerClient containerClient = null;
            try
            {
                containerClient = new BlobContainerClient(new Uri(containerUri), new ManagedIdentityCredential());
                // Create the container if it does not exist.
                containerClient.CreateIfNotExists();
            } catch (Exception ex)
            {
                throw new Exception("Accessing container for write has failed.\n" + ex.Message, ex);
            }

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(filePath);            

            Console.WriteLine("Uploading file to container. Blob URL: {0}", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(filePath);
            try
            {
                blobClient.Upload(uploadFileStream);
                return "Success";
            } catch (Exception ex)
            {
                uploadFileStream.Close();
                throw new Exception("Upload to container failed.\n" + ex.Message, ex);                
            } finally
            {
                uploadFileStream.Close();
            }
        }
    }
}
