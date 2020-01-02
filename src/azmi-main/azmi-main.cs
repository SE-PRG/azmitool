﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;

using Azure.Storage.Blobs;
using Azure.Identity;
using System.Net.Http;

namespace azmi_main
{
    
    public class HelpMessage
    {
        // TODO: Use definition above both in command line and tests projects
        // TODO: Add definition of command arguments (i.e. count, names, description)

        public static string[] supportedSubCommands = { "gettoken", "setblob" };

        public static string[] application()
        {
            var response = new List<string>() { @"
Command-line utility azmi stands for Azure Managed Identity.
It is helping admins simplify common operations (reading / writing) on standard Azure resources.
It is utilizing Azure AD authentication via user assigned managed identity.

Usage:
  azmi help - displays this help message" };
            foreach (var subCommand in supportedSubCommands)
            {                
                response.Add($"  azmi {subCommand} help - displays help on {subCommand} sub-command");                
            }            
            return response.ToArray();
        }

        public static string[] subCommand(string commandName)
        {
            if (commandName == "setblob")
            {
                return new string[] { @"
Subcommand 'setblob' is used for writing to storage account blob.

Usage:
  azmi setblob help - displays this help message
  azmi setblob $FILE $CONTAINER - writes a local file to a storage account container" };            
            }
            else if (commandName == "gettoken")
            {
                return new string[] { @"
Subcommand 'gettoken' is used for obtaining Azure authorization token.

Usage:
  azmi gettoken help - displays this help message              
  azmi gettoken [$ENDPOINT] obtains token against management (default value) or storage endpoints" };
            }
            else
            {
                throw new ArgumentNullException($"Unknown help for subcommand '{commandName}'.");
            }
        }
    }

    public class Operations
    {

        // Class defining main operations performed by azmi tool

        public static string metadataUri (string endpoint = "management", string apiVersion = "2018-02-01")
        {
            string[] validEndpoints = { "management", "storage"};
            if (!(validEndpoints.Contains(endpoint)))
            {
                throw new ArgumentOutOfRangeException($"Metadata endpoint '{endpoint}' not supported.");
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
                    throw new ArgumentNullException("Received empty response from metaData service.");
                }
                else
                {
                    return metaDataResponse;
                }
            }
            catch (Exception e)
            {                                
                throw new Exception("Failed to receive response from metadata. " + e.Message);
            }
        }

        public static string extractToken(string metaDataResponse)
        {
            try
            {
                var obj = (Dictionary<string, string>)JsonSerializer.Deserialize(metaDataResponse, typeof(Dictionary<string, string>));
                return obj["access_token"];
            }
            catch (Exception e)
            {
                throw new Exception("Could not deserialize access token. " + e.Message);
            }
        }

        public static string getToken(string endpoint = "")
        {
            // Method unifies above two mentioned methods into one
            return extractToken(getMetaDataResponse(endpoint));
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
            BlobContainerClient containerClient = new BlobContainerClient(new Uri(containerUri), new ManagedIdentityCredential());

            // Create the container if it does not exist.
            containerClient.CreateIfNotExists();

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(filePath);

            Console.WriteLine("Uploading to Blob storage as blob: {0}", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(filePath);
            try
            {                
                blobClient.Upload(uploadFileStream);
                return "OK";
            } catch
            {
                uploadFileStream.Close(); 
                return "NOT OK";
            }
            finally
            {
                uploadFileStream.Close();
            }
        }
    }

}
