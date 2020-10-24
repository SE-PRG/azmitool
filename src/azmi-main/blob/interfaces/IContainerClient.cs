using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace azmi_main
{
    public interface IContainerClient
    {
        Pageable<BlobItem> GetBlobs(string prefix);
        void CreateIfNotExists();
        BlobClient GetBlobClient(string blobName);
    }
}
