using System;
using Xunit;
using azmi_main;
using System.Linq;



namespace azmi_tests
{
    public class UnitTest1
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
            Assert.Contains(helpResponse, s => s.Contains("setblob help"));
            // TODO: Do commands check programatically
        }


        //
        // extractToken tests
        //

        [Fact]
        public void extractToken_returnsResponseOnAzureVM()
        {
            // TODO: Mock http call result
            Assert.True(true);
        }

        [Fact]
        public void extractToken_throwsOnNonAzureVM()
        {
            // TODO: Mock http call result
            Assert.True(true);
        }

    }
}
