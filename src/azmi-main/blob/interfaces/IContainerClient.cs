using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public interface IContainerClient
    {
        Pageable<BlobItem> GetBlobs(string prefix);
        void CreateIfNotExists();


        BlobClient GetBlobClient(string blobName);

    }
}
