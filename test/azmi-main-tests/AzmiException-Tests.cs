using System;
using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class AzmiException_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class BaseConstructor_TestsGroup
        {
            //
            // base constructors
            //

            [Fact]
            public void BaseConstructor_NoArguments()
            {
                var a = new AzmiException();
                Assert.IsType<AzmiException>(a);
            }

            [Fact]
            public void BaseConstructor_Type()
            {
                var a = new AzmiException("some-message");
                Assert.IsType<AzmiException>(a);
            }

            [Fact]
            public void BaseConstructor_Message()
            {
                var a = new AzmiException("some-message");
                Assert.Equal("some-message", a.Message);
            }

            [Fact]
            public void BaseConstructor_InnerExceptionMessage()
            {
                var innerEx = new AzmiException("inner-message");
                var outerEx = new AzmiException("outer-message", innerEx);
                Assert.Equal("inner-message", outerEx.InnerException.Message);
            }

            [Fact]
            public void BaseConstructor_InnerExceptionType()
            {
                var innerEx = new ArgumentException("some-message");
                var outerEx = new AzmiException("outer-message", innerEx);
                Assert.IsType<ArgumentException>(outerEx.InnerException);
            }
        }


        public class WrongObject_TestsGroup
        {
            //
            // Check WrongObject method
            //

            [Fact]
            public void WrongObject_Works()
            {
                Assert.NotNull(AzmiException.WrongObject(null));
            }

            [Fact]
            public void WrongObject_ProperInnerMessage()
            {
                var innerEx = new AzmiException("inner-text");
                var outerEx = AzmiException.WrongObject(innerEx);
                Assert.Equal("inner-text", outerEx.InnerException.Message);
            }

            [Fact]
            public void WrongObject_ProperInnerType()
            {
                var innerEx = new ArgumentException("inner-text");
                var outerEx = AzmiException.WrongObject(innerEx);
                Assert.IsType<ArgumentException>(outerEx.InnerException);
            }

        }

        public class IDCheck_TestsGroup
        {
            //
            // ID check
            //

            // TODO: This must be used with mock on GetToken
        }


    }
}
