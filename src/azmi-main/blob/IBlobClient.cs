
using Azure;
using Azure.Storage.Blobs.Models;

namespace azmi_main
{
    interface IBlobClient
    {
        Response<BlobContentInfo> Upload(string path, bool overwrite = false);
    }
}
