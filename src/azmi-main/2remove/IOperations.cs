using System.Collections.Generic;

namespace azmi_main
{
    interface IOperations
    {
        string getToken(string endpoint = "management", string identity = null, bool JWTformat = false);
        string getBlob(string blobURL, string filePath, string identity = null, bool ifNewer = false, bool deleteAfterCopy = false);
        List<string> getBlobs(string containerUri, string directory, string identity = null, string prefix = null, string exclude = null, bool ifNewer = false, bool deleteAfterCopy = false);
        List<string> listBlobs(string containerUri, string identity = null, string prefix = null, string exclude = null);
        string setBlob_byContainer(string filePath, string containerUri, string identity = null, bool force = false);
        string setBlob_byBlob(string filePath, string blobUri, string identity = null, bool force = false);
        string getSecret(string secretIdentifierUrl, string identity = null);
    }
}
