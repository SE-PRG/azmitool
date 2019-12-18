using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

namespace azmi_main
{
    public class HelpMessage
    {
        // TODO: Add definition of implemented commands (string array) and use it programatically
        // TODO: Use definition above both in command line and tests projects
        // TODO: Add definition of command arguments (i.e. count, names, description)

        public static string[] application()
        {
            return new string[] { "Usage:", "Add more explanation" };
        }

        public static string[] command(string commandName)
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

        public static string getMetaDataResponse()
        {
            // TODO: Extend this to support also provided managed identity name
            // TODO: This should also support different endpoints except management, like storage

            // Build request to acquire managed identities for Azure resources token
            string metaDataUri = "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://management.azure.com/"
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(metaDataUri);
            request.Headers["Metadata"] = "true";
            request.Method = "GET";

            try
            {
                // Call /token endpoint
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader, and extract access token
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                return streamResponse.ReadToEnd();
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
                Dictionary<string, string> obj = (Dictionary<string, string>)JsonSerializer.Deserialize(metaDataResponse, typeof(Dictionary<string, string>));
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

        public static void setBlob(string blobUri, string filePath)
        {
            // sets blob content based on local file content
        }
    }

}
