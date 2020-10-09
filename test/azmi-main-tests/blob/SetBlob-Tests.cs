using azmi_main;
using Azure;
using Azure.Storage.Blobs.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace azmi_tests
{
    public class SetBlob_Tests
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
                var subCommand = new SetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new SetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }

        public class RealArguments_TestsGroup
        {
            // test if it has real arguments we defined, prevents accidental argument removal
            [Theory]
            [InlineData("file")]
            [InlineData("blob")]
            [InlineData("force")]
            public void TestRealArguments(string argName)
            {
                // Arrange
                var subCommand = new SetBlob();
                // Act
                var arguments = subCommand.Definition().arguments;
                // Assert
                Assert.NotEmpty(arguments.Where(a => a.name == argName));
            }

            [Fact]
            public void NoFakeArgument()
            {
                var subCommand = new SetBlob();
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

            // test if it will call real execute method
            [Fact]
            public void FailsWithNonExistingProperty()
            {
                var obj = new { nonExistingProperty = 1 };
                var subCommand = new SetBlob();

                // it throws exception
                var actualExc = Assert.Throws<AzmiException>(() =>
                   subCommand.Execute(obj)
                );
                Assert.Equal(_failMsg, actualExc.Message);
            }

            [Fact]
            public void FailsWithExistingProperty()
            {
                var obj = new SetBlob.AzmiArgumentsClass
                {
                    file = _anyGoodPath,
                    blob = _anyValidURL,
                    force = _force,
                    identity = _identity,
                    verbose = false
                };
                var subCommand = new SetBlob();

                // it throws exception
                var actualExc = Assert.Throws<FileNotFoundException>(
                    () => subCommand.Execute(obj)
                );
                Assert.NotEqual(_failMsg, actualExc.Message);
                Assert.Contains(_anyGoodPath, actualExc.Message);
            }
        }


        //
        // main tests
        //

        public class SetBlobExecute_TestsGroup
        {
            // mock Upload method to return success or failure and check SetBlob status

            // setup fake variables, easier than using Any constructs
            private readonly string _anyGoodPath = "a.txt";
            private readonly string _identity = "123";
            private readonly string _anyValidURL = "https://www.example.com";
            private readonly bool _force = false;
            private readonly Exception _testException = new Exception("testing exception");
            private readonly Response<BlobContentInfo> _bci = null;

            [Fact]
            public void ExecutedWithSuccess()
            {
                // mock with success
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_anyGoodPath, _force).Returns(_bci);
                var subCommand = new SetBlob(blobSubstitute);

                var retValue = subCommand.Execute(_anyGoodPath, _anyValidURL, _identity, _force);
                Assert.Equal("Success", retValue);

            }

            [Fact]
            public void ExecutedWithFailure_WithIdentity()
            {
                // mock with exception and call with identity
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_anyGoodPath, _force).Throws(_testException);
                var subCommand = new SetBlob(blobSubstitute);

                // it returns testing exception
                var actualExc = Assert.Throws<Exception>(
                    () => subCommand.Execute(_anyGoodPath, _anyValidURL, _identity, _force)
                );
                Assert.Equal(_testException.Message, actualExc.Message);
            }

            [Fact]
            public void ExecutedWithFailure_NoIdentity()
            {
                // mock with exception and call with no identity
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_anyGoodPath, _force).Throws(_testException);
                var subCommand = new SetBlob(blobSubstitute);

                // it returns Azmi exception and testing one as inner
                var actualExc = Assert.Throws<AzmiException>(
                    () => subCommand.Execute(_anyGoodPath, _anyValidURL, null, _force)
                );
                Assert.Equal(_testException.Message, actualExc.InnerException.Message);

            }
        }


    }
}
