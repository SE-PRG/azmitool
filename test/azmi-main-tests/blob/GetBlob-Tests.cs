using azmi_main;
using Azure;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace azmi_tests
{
    public class GetBlob_Tests
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
                var subCommand = new GetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new GetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }

        public class RealArguments_TestsGroup
        {
            // test if it has real arguments we defined, prevents accidental argument removal
            [Theory]
            [InlineData("blob")]
            [InlineData("file")]
            [InlineData("if-newer")]
            [InlineData("delete-after-copy")]
            public void TestRealArguments(string argName)
            {
                // Arrange
                var subCommand = new GetBlob();
                // Act
                var arguments = subCommand.Definition().arguments;
                // Assert
                Assert.NotEmpty(arguments.Where(a => a.name == argName));
            }

            [Fact]
            public void NoFakeArgument()
            {
                var subCommand = new GetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.Empty(arguments.Where(a => a.name == "fakeArgument"));
            }
        }
        public class GenericExecute_TestsGroup
        {
            private readonly string _anyGoodPath = "a.txt";
            private readonly string _identity = "123";
            private readonly Uri _anyValidURL = new Uri("https://www.example.com");
            private readonly bool _force = false;
            private readonly string _failMsg = "Cannot convert input object to proper class";
            private readonly Response _nullResponse = null;

            // test if it will call real execute method
            [Fact]
            public void FailsWithNonExistingProperty()
            {
                var obj = new { nonExistingProperty = 1 };
                var subCommand = new GetBlob();

                // it throws exception
                var actualExc = Assert.Throws<AzmiException>(() =>
                   subCommand.Execute(obj)
                );
                Assert.Equal(_failMsg, actualExc.Message);
            }

            // TODO: Add test that will fail with existing property, but with different exception than above
            [Fact]
            public void WorksWithExistingProperties()
            {
                // Arrange
                var obj = new GetBlob.AzmiArgumentsClass
                {
                    file = _anyGoodPath,
                    blob = _anyValidURL,
                    ifNewer = false,
                    deleteAfterCopy = false,
                    identity = _identity,
                    verbose = false
                };

                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Returns(_nullResponse);
                var subCommand = new GetBlob(blobSubstitute);

                // Act and Assert
                var retValue = subCommand.Execute(obj);
                Assert.Equal("Success", retValue.First());
                blobSubstitute.DidNotReceive().Delete();
            }
        }


        //
        // main tests
        //

        public class GetBlobExecute_TestsGroup
        {
            // mock DownloadTo method to return success or failure and check SetBlob status
            private readonly string _anyGoodPath = "a.txt";
            private readonly string _identity = "123";
            private readonly Uri _anyValidURL = new Uri("https://www.example.com");
            private readonly Response _nullResponse = null;
            private readonly Exception _testException = new Exception("testing exception");
            private readonly Exception _testAzureException = new Azure.RequestFailedException("testing Azure exception");

            [Fact]
            public void ExecutedWithSuccess()
            {
                // mock with success

                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Returns(_nullResponse);
                var subCommand = new GetBlob(blobSubstitute);
                // Act
                var retValue = subCommand.Execute(_anyValidURL, _anyGoodPath, _identity, false, false);
                // Assert
                Assert.Equal("Success", retValue);
                blobSubstitute.DidNotReceive().Delete();
            }

            [Fact]
            public void ExecutedWithFailure_WithIdentity()
            {
                // mock with exception and call with identity

                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Throws(_testException);
                var subCommand = new GetBlob(blobSubstitute);
                // Act & Assert
                var actualExc = Assert.Throws<Exception>(
                    () => subCommand.Execute(_anyValidURL, _anyGoodPath, _identity, false, false)
                );
                Assert.Equal(_testException.Message, actualExc.Message);
            }

            [Fact]
            public void ExecutedWithFailure_NoIdentity()
            {
                // mock with exception and call without identity

                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Throws(_testException);
                var subCommand = new GetBlob(blobSubstitute);
                // Act & Assert
                var actualExc = Assert.Throws<AzmiException>(
                    () => subCommand.Execute(_anyValidURL, _anyGoodPath, null)
                );
                Assert.Equal(_testException.Message, actualExc.InnerException.Message);
            }

            [Fact]
            public void ExecutedWithAzureFailure()
            {
                // mock with exception and call without identity

                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Throws(_testAzureException);
                var subCommand = new GetBlob(blobSubstitute);
                // Act & Assert
                var actualExc = Assert.Throws<RequestFailedException>(
                    () => subCommand.Execute(_anyValidURL, _anyGoodPath, null)
                );
                Assert.Equal(_testAzureException.Message, actualExc.Message);
            }

            [Fact]
            public void DeleteAfterCopyShouldBeCalled()
            {
                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Returns(_nullResponse);
                var subCommand = new GetBlob(blobSubstitute);
                // Act & Assert
                subCommand.Execute(_anyValidURL, _anyGoodPath, _identity, deleteAfterCopy: true);
                blobSubstitute.Received().Delete();
            }

            [Fact]
            public void IfNewer_IgnoredForNotExistingFile()
            {
                // Arrange
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.DownloadTo(_anyGoodPath).Returns(_nullResponse);
                var subCommand = new GetBlob(blobSubstitute);
                // Act & Assert
                var retValue = subCommand.Execute(_anyValidURL, _anyGoodPath, _identity, ifNewer: true);
                Assert.NotEqual("Skipped", retValue);
            }

            // TODO: Add two more tests:
            // - IfNewer_IgnoredForNewerFile, returns Success
            // - IfNewer_AppliedForOlderFile, returns Skipped

        }
    }
}
