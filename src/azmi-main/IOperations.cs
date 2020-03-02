using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    interface IOperations
    {
        string getToken(string endpoint = "management", string identity = null);
        string getBlob(string blobURL, string filePath, string identity = null, bool ifNewer = false);
        string listBlobs(string containerUri, string identity = null, string prefix = null);
        string setBlob_byContainer(string filePath, string containerUri, bool force = false, string identity = null);
        string setBlob_byBlob(string filePath, string blobUri, bool force = false, string identity = null);
    }
}
