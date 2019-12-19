using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;

namespace azmi_main
{
    
    public class HelpMessage
    {
        // TODO: Add definition of implemented commands (string array) and use it programatically
        // TODO: Use definition above both in command line and tests projects
        // TODO: Add definition of command arguments (i.e. count, names, description)

        public static string[] supportedSubCommands = { "setblob" };

        public static string[] application()
        {
            var response = new List<string>() { "Usage:", "help - displays this help" };
            foreach (var subCommand in supportedSubCommands)
            {                
                response.Add($"azmi {subCommand} help - displays help on {subCommand} command");                
            }            
            return response.ToArray();
        }

        public static string[] subCommand(string commandName)
        {
            if (commandName == "setblob")
            {
                return new string[] { "Usage:", "Add more explanation" };            
            }
            else
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

        public static string getMetaDataResponse(string endpoint = null)
        {
            // TODO: Extend this to support also provided managed identity name
            // TODO: This should also support different endpoints except management, like storage

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

        public static string getToken()
        {
            // Method unifies above two mentioned methods into one
            return extractToken(getMetaDataResponse());
        }

        public static string setBlob(string blobUri, string filePath)
        {
            // sets blob content based on local file content
            // TODO: Implement set blob method!
            //return getToken();
            return (getToken());
        }
    }

}
