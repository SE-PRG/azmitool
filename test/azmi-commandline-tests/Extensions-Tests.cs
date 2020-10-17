using azmi_commandline;
using azmi_main;
using System.CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine.Invocation;
using Xunit;

namespace azmi_commandline_tests
{
    public class Extensions_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class OptionNames_TestsGroup
        {
            [Fact]
            public void OptionNames_NoAlias()
            {
                var a = new AzmiArgument("some-name", alias: null, "some-descrtiption");
                Assert.Single(a.OptionNames());
            }

            [Fact]
            public void OptionNames_WithAlias()
            {
                var a = new AzmiArgument("some-name", alias: 's', "some-descrtiption");
                Assert.Equal(2, a.OptionNames().Length);
            }
        }

        public class OptionArgument_TestsGroup
        {
            [Fact]
            public void OptionArgument_String()
            {
                var a = new AzmiArgument("some-name", ArgType.str);
                Assert.Equal("string", a.OptionArgument().Name);
            }

            [Fact]
            public void OptionArgument_Bool()
            {
                var a = new AzmiArgument("some-name", ArgType.flag);
                Assert.Equal("bool", a.OptionArgument().Name);
            }

            [Fact]
            public void OptionArgument_Url()
            {
                var a = new AzmiArgument("some-name", ArgType.url);
                Assert.Equal("url", a.OptionArgument().Name);
            }
        }

        public class OptionDescription_TestsGroup
        {
            [Fact]
            public void OptionDescription_Required()
            {
                var a = new AzmiArgument("some-name", "some-description", required: true);
                Assert.Equal("Required. some-description", a.OptionDescription());
            }

            [Fact]
            public void OptionDescription_Optional()
            {
                var a = new AzmiArgument("some-name", "some-description", required: false);
                Assert.Equal("Optional. some-description", a.OptionDescription());
            }
        }

        public class ToOption_TestsGroup
        {
            [Fact]
            public void ToOption_ProperType()
            {
                var a = new AzmiArgument("some-name");
                Assert.IsType<Option>(a.ToOption());
            }


            [Theory]
            [InlineData(ArgType.flag, "bool")]
            [InlineData(ArgType.str, "string")]
            [InlineData(ArgType.url, "url")]
            public void ToOption_Argument(ArgType type, string name)
            {
                var a = new AzmiArgument("some-name", type);
                Assert.Equal(name, a.ToOption().Argument.Name);
            }


            [Theory]
            [InlineData(true, "Required.")]
            [InlineData(false, "Optional.")]
            public void ToOption_Description(bool required, string prefix)
            {
                var a = new AzmiArgument("some-name", "some-description", required: required);
                Assert.Equal($"{prefix} some-description", a.ToOption().Description);
            }


            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ToOption_Required(bool required)
            {
                var a = new AzmiArgument("some-name", required: required);
                Assert.Equal(required, a.ToOption().Required);
            }

        }

        public class ToCommand_TestsGroup
        {

            private class DummyAzmiCommand : IAzmiCommand
            {
                public SubCommandDefinition Definition()
                {
                    return new SubCommandDefinition
                    {
                        name = "dummy_command",
                        arguments = new AzmiArgument[] { SharedAzmiArguments.identity }
                    };
                }

                public List<string> Execute(object options)
                {
                    return new List<string> { "dummy command executed" };
                }
            }


            [Fact]
            public void ToCommand_ProperType()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand, SharedAzmiArgumentsClass>();
                Assert.IsType<Command>(a);
            }

            [Fact]
            public void ToCommand_ProperName()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand, SharedAzmiArgumentsClass>();
                Assert.Equal("dummy_command", a.Name);
            }

            [Fact]
            public void ToCommand_ProperOptionCount()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand, SharedAzmiArgumentsClass>();
                Assert.Single(a.Options);
            }

            [Fact]
            public void ToCommand_ProperOptionName()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand, SharedAzmiArgumentsClass>();
                Assert.Equal("identity", a.Options.First<Option>().Name);
            }

            [Fact]
            public void ToCommand_HandlerProperType()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand, SharedAzmiArgumentsClass>();
                Assert.IsAssignableFrom<ICommandHandler>(a.Handler);
            }

            // TODO: Add more tests for command.handler

            private class DummyAzmiCommand2 : IAzmiCommand
            {
                public SubCommandDefinition Definition()
                {
                    return new SubCommandDefinition
                    {
                        name = "dummy_command",
                        arguments = new AzmiArgument[] {
                            SharedAzmiArguments.identity,
                            new AzmiArgument("exclude", multiValued: true),
                        }
                    };
                }

                public List<string> Execute(object options)
                {
                    return new List<string> { "dummy command executed" };
                }
            }

            [Fact]
            public void ToCommand_ProperType2()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand2, SharedAzmiArgumentsClass>();
                Assert.IsType<Command>(a);
            }

            [Fact]
            public void ToCommand_ProperOptionCount2()
            {
                var a = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand2, SharedAzmiArgumentsClass>();
                var actualCount = a.Options.Count();
                Assert.Equal(2, actualCount);
            }

            [Fact]
            public void ToCommand_ProperMultiValuedOption()
            {
                var subCommand = AzmiCommandLineExtensions.ToCommand<DummyAzmiCommand2, SharedAzmiArgumentsClass>();
                var option = subCommand.Options.First(a => a.Name == "exclude");
                var actualArity = option.Argument.Arity;
                Assert.Equal(ArgumentArity.OneOrMore.MinimumNumberOfValues, actualArity.MinimumNumberOfValues);
                Assert.Equal(ArgumentArity.OneOrMore.MaximumNumberOfValues, actualArity.MaximumNumberOfValues);
                // TODO: use one Assert above, simple Equal is not working for objects
            }


        }

        public class DisplayError_TestsGroup
        {
            // TODO: Don't know how to capture Console.Write into test
        }

        public class WriteLines_TestsGroup
        {
            // TODO: Don't know how to capture Console.Write into test
        }

    }
}
