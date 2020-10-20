
using Azure;
using Azure.Storage.Blobs.Models;

namespace azmi_main
{
    public interface IBlobClient
    {
        Response<BlobContentInfo> Upload(string path, bool overwrite = false);
        Response DownloadTo(string path);
        Response Delete();
        Response<BlobProperties> GetProperties();
    }
}
