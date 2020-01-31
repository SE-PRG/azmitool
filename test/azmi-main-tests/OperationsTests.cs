using System;
using Xunit;
using azmi_main;
using System.IO;

namespace azmi_tests
{
    public class OperationsTests
    {
        // we use custom Azure functions to mock http metadata calls
        private static string testAzureFunction = @"https://azmi-test.azurewebsites.net/api/metadata";
        private static string testAzureVMuri = $"{testAzureFunction}?type=azure";
        private static string testNonAzureVMuri = $"{testAzureFunction}?type=nonazure";

        //
        // metadataUri
        //

        [Fact]
        public void metadataUri_returnsInternalIP()
        {
            Assert.Contains("169.254.169.254", Operations.metadataUri());
        }

        [Theory]
        [InlineData("storage")]
        [InlineData("management")]
        public void metadataUri_returnsProvidedEndpoint(string endpoint)
        {
            Assert.Contains(endpoint, Operations.metadataUri(endpoint));
        }

        [Fact]
        public void metadataUri_throwsForInvalidEndpoint()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Operations.metadataUri("invalid_endpoint"));
        }

        //
        // getMetaDataResponse
        //

        [Fact]
        public void getMetaDataResponse_emptyOnNonAzureVM()
        {
            Assert.ThrowsAny<Exception>(() => Operations.getMetaDataResponse(testNonAzureVMuri));
        }

        [Fact]
        public void getMetaDataResponse_worksOnAzureVM()
        {
            Assert.NotEmpty(Operations.getMetaDataResponse(testAzureVMuri));
        }

        //
        // extractToken
        //

        [Fact]
        public void extractToken_worksWithProperJSON()
        {
            string validJSON = @"{""access_token"": ""123"", ""second_field"": ""234""}";
            Assert.Equal("123", Operations.extractToken(validJSON));
        }


        [Fact]
        public void extractToken_failsWithWrongJSON()
        {
            Assert.Throws<Exception>(() => Operations.extractToken("invalid_JSON"));
        }

        //
        // getToken
        //

        [Fact]
        public void getToken_worksOnAzureVM()
        {
            Assert.NotEmpty(Operations.getToken(testAzureVMuri));
        }

        [Fact]
        public void getToken_failsOnNonAzureVM()
        {
            Assert.ThrowsAny<Exception>(() => Operations.getToken(testNonAzureVMuri));
        }

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
            var ex = Assert.Throws<FileNotFoundException>(() => Operations.setBlob("nonexistingfile", "blobdoesnotmatter"));
            Assert.Equal("File 'nonexistingfile' not found!", ex.Message);
        }

        [Fact]
        public void setBlob_failsIfNoContainerExists()
        {
            // TODO: Check for proper exception message
            var tempFile = Path.GetTempFileName();
            File.Create(tempFile).Close();
            Assert.ThrowsAny<Exception>(() => Operations.setBlob(tempFile, "blobdoesnotmatter"));
            File.Delete(tempFile);
        }
    }
}
