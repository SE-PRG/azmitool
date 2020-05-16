using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class IAzmiCommand_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class SubCommandDefinition_TestsGroup
        {

            [Fact]
            public void SubCommandDefinition_Test()
            {
                Assert.NotNull(new SubCommandDefinition());
            }

            [Fact]
            public void SubCommandDefinition_Properties()
            {
                var a = new SubCommandDefinition()
                {
                    name = "sc-name",
                    description = "sc-description",
                    arguments = new AzmiArgument[] { SharedAzmiArguments.identity }
                };

                // TODO: Split these into three or fail only after all are executed
                Assert.Equal("sc-name", a.name);
                Assert.Equal("sc-description", a.description);
                Assert.IsType<AzmiArgument[]>(a.arguments);
            }

            [Fact]
            public void SubCommandDefinition_Name()
            {
                var a = new SubCommandDefinition() { name = "sc-name" };
                Assert.Equal("sc-name", a.name);
            }

            [Fact]
            public void SubCommandDefinition_Description()
            {
                var a = new SubCommandDefinition() { description = "sc-description" };
                Assert.Equal("sc-description", a.description);
            }

            [Fact]
            public void SubCommandDefinition_Arguments()
            {
                var a = new SubCommandDefinition() { arguments = new AzmiArgument[] { SharedAzmiArguments.identity } };
                Assert.IsType<AzmiArgument[]>(a.arguments);
            }
        }

        public class Interface_TestsGroup
        {
            // TODO: What to do here?
        }
    }
}
