using Azure;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System;
using System.Threading.Tasks;

namespace azmi_main
{
    public class BlobClientImpl : IBlobClient
    {

        private readonly BlobClient blobClient;

        public BlobClientImpl (Uri blobUri, TokenCredential credential)
        {
            blobClient = new BlobClient(blobUri, credential);
        }

        public Response<BlobContentInfo> Upload(string path, bool overwrite = false)
        {
            return blobClient.Upload(path, overwrite);
        }

        public Response Delete()
        {
            return blobClient.Delete();
        }

        public Response DownloadTo(string path)
        {
            return blobClient.DownloadTo(path);
        }

        public Task<Response> DownloadToAsync(string path)
        {
            return blobClient.DownloadToAsync(path);
        }

        public Response<BlobProperties> GetProperties()
        {
            return blobClient.GetProperties();
        }
        public Task<Response<BlobProperties>> GetPropertiesAsync()
        {
            return blobClient.GetPropertiesAsync();
        }
    }
}
