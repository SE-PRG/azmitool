using System;
using Xunit;
using azmi_main;
using System.IO;

namespace azmi_tests
{
    public class OperationsTests
    {
        //
        // getToken
        //

        // TODO: Create tests for getToken command


        //
        // getBlob
        //
        
        [Fact]
        public void getBlob_failsToDownloadOrSave()
        {
            var ex = Assert.ThrowsAny<Exception>(() => Operations.getBlob("https://INVALID_BLOB_URL.net/", "download.txt"));            
        }

        //
        // setBlob
        //

        [Fact]
        public void setBlob_failsIfNoLocalFile()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => Operations.setBlob_byContainer("nonexistingfile", "blobdoesnotmatter"));
            Assert.Equal("File 'nonexistingfile' not found!", ex.Message);
        }

        [Fact]
        public void setBlob_failsIfNoContainerExists()
        {
            // TODO: Check for proper exception message
            var tempFile = Path.GetTempFileName();
            File.Create(tempFile).Close();
            Assert.ThrowsAny<Exception>(() => Operations.setBlob_byContainer(tempFile, "blobdoesnotmatter"));
            File.Delete(tempFile);
        }
    }
}
