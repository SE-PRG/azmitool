using System.CommandLine;
using Xunit;

namespace azmi_commandline_tests
{
    public class azmi_commandline_tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class Main_TestsGroup
        {
            [Fact]
            public void Main_Works()
            {
                Assert.True(true);
                // TODO: Add real tests here
            }
        }

        public class ConfigureArguments_TestsGroup
        {
            [Fact]
            public void ConfigureArguments_ProperType()
            {
                var a = azmi_commandline.Program.ConfigureArguments();
                Assert.IsType<RootCommand>(a);
            }

            // TODO: Add more tests here, like iterate through all subcommands?

        }
    }
}
