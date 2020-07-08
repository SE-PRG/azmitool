using System.Collections.Generic;
using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class AzmiExtensions_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        [Fact]
        public void SimpleString_ProperType()
        {
            string a = "some-text";
            Assert.IsType<List<string>>(a.ToStringList());
        }

        [Fact]
        public void SimpleString_ProperValue()
        {
            string a = "some-text";
            Assert.Equal(new List<string> { a }, a.ToStringList());
        }

    }
}
