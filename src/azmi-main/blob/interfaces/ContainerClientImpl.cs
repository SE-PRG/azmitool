using System;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace azmi_main
{
    class ContainerClientImpl : IContainerClient
    {

        private readonly BlobContainerClient containerClient;

        public ContainerClientImpl(Uri containerUri, TokenCredential credential)
        {
            containerClient = new BlobContainerClient(containerUri, credential);
        }


        //
        //  Implemented Methods
        //

        Pageable<BlobItem> IContainerClient.GetBlobs(string prefix)
        {
            return containerClient.GetBlobs(prefix: prefix);
        }

        public void CreateIfNotExists()
        {
            containerClient.CreateIfNotExists();
        }

        public BlobClient GetBlobClient(string blobName)
        {
            return containerClient.GetBlobClient(blobName);
        }
    }
}
