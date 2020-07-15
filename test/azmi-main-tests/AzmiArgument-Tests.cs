using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class AzmiArgument_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class SimpleConstructor_TestsGroup
        {
            //
            // Simplest constructor, one string as name
            //

            [Fact]
            public void SimplestConstructor_Works()
            {
                Assert.NotNull(new AzmiArgument("some-name"));
            }

            [Fact]
            public void SimplestConstructor_ProperAlias()
            {
                var a = new AzmiArgument("some-name");
                Assert.Equal('s', a.alias);
            }

            [Fact]
            public void SimplestConstructor_ProperName()
            {
                var a = new AzmiArgument("some-name");
                Assert.Equal("some-name", a.name);
            }

            [Fact]
            public void SimplestConstructor_DefinesString()
            {
                var a = new AzmiArgument("some-name");
                Assert.Equal(ArgType.str, a.type);
            }

            [Fact]
            public void SimplestConstructor_DefinesOptionalArgument()
            {
                var a = new AzmiArgument("some-name");
                Assert.False(a.required);
            }

            [Fact]
            public void SimplestConstructor_DefinesSingleValued()
            {
                var a = new AzmiArgument("some-name");
                var isMultiValued = a.multiValued;
                Assert.False(isMultiValued);

            }
        }

        public class TwoStringsConstructor_TestsGroup
        {
            //
            // Two strings constructor, name and description
            //

            [Fact]
            public void TwoStringsConstructor_Works()
            {
                Assert.NotNull(new AzmiArgument("some-name", "some-description"));
            }

            [Fact]
            public void TwoStringsConstructor_ProperAlias()
            {
                var a = new AzmiArgument("some-name", "some-description");
                Assert.Equal('s', a.alias);
            }

            [Fact]
            public void TwoStringsConstructor_ProperDescription()
            {
                var a = new AzmiArgument("some-name", "some-description");
                Assert.Equal("some-description", a.description);
            }

            [Fact]
            public void TwoStringsConstructor_DefinesString()
            {
                var a = new AzmiArgument("some-name", "some-description");
                Assert.Equal(ArgType.str, a.type);
            }

            [Fact]
            public void TwoStringsConstructor_DefinesOptionalArgument()
            {
                var a = new AzmiArgument("some-name", "some-description");
                Assert.False(a.required);
            }

            [Fact]
            public void TwoStringsConstructor_OptionalType()
            {
                var a = new AzmiArgument("some-name", "some-description", ArgType.flag);
                Assert.Equal(ArgType.flag, a.type);
            }

            [Fact]
            public void TwoStringsConstructor_OptionalRequired()
            {
                var a = new AzmiArgument("some-name", "some-description", required: true);
                Assert.True(a.required);
            }

            [Fact]
            public void TwoStringsConstructor_OptionalMultiValuedIsTrue()
            {
                var a = new AzmiArgument("some-name", "some-description", multiValued: true);
                var isMultiValued = a.multiValued;
                Assert.True(isMultiValued);
            }

            [Fact]
            public void TwoStringsConstructor_OptionalMultiValuedIsFalse()
            {
                var a = new AzmiArgument("some-name", "some-description");
                var isMultiValued = a.multiValued;
                Assert.False(isMultiValued);
            }
        }

        public class ThreeArgumentsConstructor_TestsGroup
        {
            //
            // Two strings and a char constructor, name, description and alias
            //

            [Fact]
            public void ThreeArgsConstructor_Works()
            {
                Assert.NotNull(new AzmiArgument("some-name", 's', "some-description"));
            }

            [Fact]
            public void ThreeArgsConstructor_ProperAlias()
            {
                var a = new AzmiArgument("some-name", 'u', "some-description");
                Assert.Equal('u', a.alias);
            }

            [Fact]
            public void ThreeArgsConstructor_DefinesString()
            {
                var a = new AzmiArgument("some-name", 'u', "some-description");
                Assert.Equal(ArgType.str, a.type);
            }

            [Fact]
            public void ThreeArgsConstructor_DefinesOptionalArgument()
            {
                var a = new AzmiArgument("some-name", 'u', "some-description");
                Assert.False(a.required);
            }

            [Fact]
            public void ThreeArgsConstructor_AcceptsMultiValued()
            {
                var a = new AzmiArgument("some-name", 'u', "some-description", multiValued: true);
                var isMultiValued = a.multiValued;
                Assert.True(isMultiValued);
            }
        }
    }
}
