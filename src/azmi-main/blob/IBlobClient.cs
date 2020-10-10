using Azure;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;

namespace azmi_main
{
    public interface IBlobClient
    {
        Response<BlobContentInfo> Upload(string path, bool overwrite = false);
        Response DownloadTo(string path);
        Task<Response> DownloadToAsync(string path);
        Response Delete();
        Response<BlobProperties> GetProperties();
        Task<Response<BlobProperties>> GetPropertiesAsync();
    }
}
