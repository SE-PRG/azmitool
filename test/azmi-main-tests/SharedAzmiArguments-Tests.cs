using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class SharedAzmiArguments_Tests
    {
        public class SharedAzmiArgumentsClass_TestGroup
        {

            [Fact]
            public void SharedAzmiArgumentsClass_Test()
            {
                Assert.NotNull(new SharedAzmiArgumentsClass());
            }

            [Fact]
            public void SharedAzmiArgumentsClass_Identity()
            {
                var a = new SharedAzmiArgumentsClass() { identity = "id" };
                Assert.Equal("id", a.identity);
            }

            [Fact]
            public void SharedAzmiArgumentsClass_Verbose()
            {
                var a = new SharedAzmiArgumentsClass() { verbose = true };
                Assert.True(a.verbose);
            }

            [Fact]
            public void SharedAzmiArgumentsClass_NonExistingProperty()
            {
                var a = new SharedAzmiArgumentsClass();
                Assert.Null(a.GetType().GetProperty("non_existing"));
            }
        }

        public class SharedAzmiArguments_TestGroup
        {

            [Fact]
            public void SharedAzmiArguments_Identity()
            {
                Assert.NotNull(SharedAzmiArguments.identity);
            }

            [Fact]
            public void SharedAzmiArgumentsClass_Verbose()
            {
                Assert.NotNull(SharedAzmiArguments.verbose);
            }
        }
    }
}
