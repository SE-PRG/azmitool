using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;

using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;

namespace azmi_main
{
    
    public class HelpMessage
    {
        // TODO: Add definition of implemented commands (string array) and use it programatically
        // TODO: Use definition above both in command line and tests projects
        // TODO: Add definition of command arguments (i.e. count, names, description)

        public static string[] supportedSubCommands = { "gettoken", "setblob" };

        public static string[] application()
        {
            var response = new List<string>() { @"
Command-line utiliti azmi stands for Azure Managed Identity.
It is helping admins simplify common operations (reading / writing) on standard  Azure resources.
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
Subcommand setblob is used for writing to storage account blob.
Usage:
  azmi setblob help - displays this help message
  azmi setblob $CONTAINER $FILE - writes a file to storage account container"};            
            }
            else if (commandName == "gettoken")
            {
                return new string[] { @"
Subcommand setblob is used for writing to storage account blob.
Usage:
  azmi setblob help - displays this help message              
  azmi setblob $CONTAINER $FILE - writes a file to storage account container" };
            } else
            {
                throw new ArgumentNullException();
            }
        }
    }

    public class Operations
    {

        // Class defining main operations performed by azmi tool

        public static string metadataUri (string endpoint = "management", string apiVersion = "2018-02-01")
        {
            string[] validEndoints = { "management","storage"};
            if (!(validEndoints.Contains(endpoint)))
            {
                throw new Exception();
            }

            string uri = "http://169.254.169.254/metadata/identity/oauth2/token";
            uri += $"?api-version={apiVersion}";
            uri += $"&resource=https://{endpoint}.azure.com";

            return uri;
        }

        public static string getMetaDataResponse(string endpoint = "management")
        {
            // TODO: Extend this to support also provided managed identity name

            // Build request to acquire managed identities for Azure resources token
            var request = (HttpWebRequest)WebRequest.Create(metadataUri(endpoint));
            request.Headers["Metadata"] = "true";
            request.Method = "GET";
            // TODO: Switch to HttpClient
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest?view=netframework-4.8#remarks

            try
            {
                // Call /token endpoint
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                var metaDataResponse = streamResponse.ReadToEnd();
                if (String.IsNullOrEmpty(metaDataResponse))
                {
                    throw new Exception();
                }
                else
                {
                    return metaDataResponse;
                }
                
            } catch (Exception e)
            {
                string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Acquire token failed");
                //TODO: Throw proper error
                throw new Exception();
            }
        }

        public static string extractToken(string metaDataResponse)
        {
            try
            {
                var obj = (Dictionary<string, string>)JsonSerializer.Deserialize(metaDataResponse, typeof(Dictionary<string, string>));
                return obj["access_token"];
            }
            catch
            {
                //TODO: Throw proper error
                throw new Exception();
            }
        }

        public static string getToken(string endpoint = "management")
        {
            // Method unifies above two mentioned methods into one
            return extractToken(getMetaDataResponse(endpoint));
        }

        public static string setBlob(string containerUri, string filePath)
        {
            // sets blob content based on local file content
            // TODO: Test / handle if we provide filename as /dir/file.name
            // TODO: Test / handle if we provide container as directory within it
            //       like mycontainer/mydir at the end

            string fileName;
            string localFilePath;

            if (File.Exists(filePath))
            {
                localFilePath = Path.GetFullPath(filePath);
                fileName = Path.GetFileName(localFilePath);
            } else
            {
                throw new Exception();
            }
            
            var containerEndpoint = new Uri(containerUri);

            // Get a credential and create a client object for the blob container.
            BlobContainerClient containerClient = new BlobContainerClient(containerEndpoint, new ManagedIdentityCredential());

            // Create the container if it does not exist.
            containerClient.CreateIfNotExists();

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            blobClient.Upload(uploadFileStream);
            
            uploadFileStream.Close();
            
            return ("OK");
        }
    }

}
