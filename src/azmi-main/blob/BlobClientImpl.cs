using System;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

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
            return this.blobClient.Upload(path, overwrite);
        }
    }
}
