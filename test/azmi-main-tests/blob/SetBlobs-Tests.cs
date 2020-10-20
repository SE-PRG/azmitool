using azmi_main;
using Azure;
using Azure.Storage.Blobs.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Linq;
using Xunit;

namespace azmi_tests
{
    public class SetBlobs_Tests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }

        public class SharedArguments_TestsGroup
        {
            [Fact]
            public void IdentityArgument()
            {
                var subCommand = new SetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new SetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }

        public class RealArguments_TestsGroup
        {
            // test if it has real arguments we defined, prevents accidental argument removal
            [Theory]
            [InlineData("directory")]
            [InlineData("container")]
            [InlineData("force")]
            [InlineData("exclude")]
            public void TestRealArguments(string argName)
            {
                // Arrange
                var subCommand = new SetBlobs();
                // Act
                var arguments = subCommand.Definition().arguments;
                // Assert
                Assert.NotEmpty(arguments.Where(a => a.name == argName));
            }

            [Fact]
            public void NoFakeArgument()
            {
                var subCommand = new SetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.Empty(arguments.Where(a => a.name == "fakeArgument"));
            }
        }

        public class GenericExecute_TestsGroup
        {
            private readonly string _anyGoodPath = "a.txt";
            private readonly string _identity = "123";
            private readonly string _anyValidURL = "https://www.example.com";
            private readonly bool _force = false;
            private readonly string _failMsg = "Cannot convert input object to proper class";
            private readonly Exception _testException = new Exception("testing exception");

            // test if it will call real execute method
            [Fact]
            public void FailsWithNonExistingProperty()
            {
                var obj = new { nonExistingProperty = 1 };
                var subCommand = new SetBlobs();

                // it throws exception
                var actualExc = Assert.Throws<AzmiException>(() =>
                   subCommand.Execute(obj)
                );
                Assert.Equal(_failMsg, actualExc.Message);
            }


            //[Fact]
            //public void WorksWithExistingProperties()
            //{
            //    // Arrange
            //    var obj = new SetBlobs.AzmiArgumentsClass
            //    {
            //        container = _anyValidURL,
            //        directory = _anyGoodPath,
            //        identity = _identity,
            //        verbose = false
            //    };

            //    var containerSubstitute = Substitute.For<IContainerClient>();
            //    containerSubstitute.GetBlobs(prefix: null).Returns(_nullResponse);
            //    var subCommand = new SetBlobs(containerSubstitute);

            //    // Act and Assert
            //    var retValue = subCommand.Execute(obj);
            //    Assert.Equal("Success", retValue.First());
            //    containerSubstitute.DidNotReceive().Delete();
            //}
        }

    }
}
