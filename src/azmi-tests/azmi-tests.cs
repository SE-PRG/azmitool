using System;
using Xunit;
using azmi_main;
using System.Linq;
using System.IO;

namespace azmi_tests
{
    public class HelpMessageTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            Assert.True(true);
        }

        //
        // HelpMessage tests
        //

        [Fact]
        public void HelpMessage_ExistsForApplication()
        {
            var helpResponse = HelpMessage.application();
            Assert.Contains(helpResponse, s => s.Contains("Usage"));
            foreach (var subCommand in HelpMessage.supportedSubCommands)
            {
                Assert.Contains(helpResponse, s => s.Contains($"{subCommand} help"));
            }
        }

        [Fact]
        public void HelpMessage_ExistsForEachSubcommand()
        {
            foreach (var subCommand in HelpMessage.supportedSubCommands)
            {
                var helpResponse = HelpMessage.subCommand(subCommand);
                Assert.Contains(helpResponse, s => s.Contains($"{subCommand} help"));
            }
        }
    }

    public class OperationsTests
    {
        
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
            Assert.Throws<Exception>(() => Operations.metadataUri("invalid_endpoint"));
        }

        //
        // getMetaDataResponse
        //

        [Fact]
        public void getMetaDataResponse_throwsOnNonAzureVM()
        {
            // TODO: Fix test using Mock
            Assert.True(true);
        }

        [Fact]
        public void getMetaDataResponse_worksOnAzureVM()
        {
            // TODO: Fix test using Mock
            Assert.True(true);
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
            // TODO: Fix test using Mock
            Assert.True(true);
        }

        [Fact]
        public void getToken_failsOnNonAzureVM()
        {
            // TODO: Fix test using Mock
            Assert.True(true);
        }

        //
        // setBlob
        //

        [Fact]
        public void setBlob_failsIfNoLocalFile()
        {
            var ex = Assert.Throws<Exception>(() => Operations.setBlob("nonexistingfile", "blobdoesnotmatter"));
            Assert.Equal("File nonexistingfile not found!", ex.Message);
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
