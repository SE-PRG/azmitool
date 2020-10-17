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
    public class GetBlobs_Tests
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
                var subCommand = new GetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new GetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }

        public class RealArguments_TestsGroup
        {
            // test if it has real arguments we defined, prevents accidental argument removal
            [Theory]
            [InlineData("container")]
            [InlineData("directory")]
            [InlineData("if-newer")]
            [InlineData("prefix")]
            [InlineData("delete-after-copy")]
            [InlineData("exclude")]
            // TODO: Uncomment this later [InlineData("absolute-paths")]
            public void TestRealArguments(string argName)
            {
                // Arrange
                var subCommand = new GetBlobs();
                // Act
                var arguments = subCommand.Definition().arguments;
                // Assert
                Assert.NotEmpty(arguments.Where(a => a.name == argName));
            }

            [Fact]
            public void NoFakeArgument()
            {
                var subCommand = new GetBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.Empty(arguments.Where(a => a.name == "fakeArgument"));
            }
        }

        public class GenericExecute_TestsGroup
        {
            private readonly string _anyGoodPath = "a.txt";
            private readonly string _identity = "123";
            private readonly Uri _anyValidURL = new Uri("https://www.example.com");
            private readonly string _failMsg = "Cannot convert input object to proper class";
            private readonly Exception _testException = new Exception("testing exception");

            // test if it will call real execute method
            [Fact]
            public void FailsWithNonExistingProperty()
            {
                var obj = new { nonExistingProperty = 1 };
                var subCommand = new GetBlobs();

                // it throws exception
                var actualExc = Assert.Throws<AzmiException>(() =>
                   subCommand.Execute(obj)
                );
                Assert.Equal(_failMsg, actualExc.Message);
            }

            [Fact]
            public void FailsInsideWithGoodProperties()
            {
                // Arrange
                var obj = new GetBlobs.AzmiArgumentsClass
                {
                    container = _anyValidURL,
                    directory = _anyGoodPath,
                    prefix = null,
                    exclude = null,
                    identity = null
                };

                var containerSubstitute = Substitute.For<IContainerClient>();
                containerSubstitute.GetBlobs(null).Throws(_testException);
                var subCommand = new GetBlobs(containerSubstitute);

                // Act & Assert
                var actualExc = Assert.Throws<Exception>(
                    () => subCommand.Execute(obj)
                );
                Assert.Equal(_testException.Message, actualExc.Message);
            }


            // TODO: Fix issue with accessing FileSystem and uncomment test below
            //[Fact]
            //public void WorksWithExistingProperties()
            //{
            //    // Arrange
            //    var obj = new GetBlobs.AzmiArgumentsClass
            //    {
            //        container = _anyValidURL,
            //        directory = _anyGoodPath,
            //        ifNewer = false,
            //        deleteAfterCopy = false,
            //        identity = _identity,
            //        verbose = false
            //    };
            //    // TODO: We are creating a directory here!
            //    var _zeroBlobs = Substitute.For<Pageable<BlobItem>>();
            //    var containerSubstitute = Substitute.For<IContainerClient>();
            //    containerSubstitute.GetBlobs(prefix: null).Returns(_zeroBlobs);
            //    var subCommand = new GetBlobs(containerSubstitute);

            //    // Act and Assert
            //    var retValue = subCommand.Execute(obj);
            //    Assert.Empty(retValue);
            //}
        }

    }
}
