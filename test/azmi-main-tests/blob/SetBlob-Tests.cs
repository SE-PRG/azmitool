using azmi_main;
using System.Linq;
using Xunit;
using NSubstitute;
using Azure.Storage.Blobs.Models;
using Azure;
using NSubstitute.ExceptionExtensions;
using System;
using System.IO;

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
                Assert.NotNull(arguments.Select(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new SetBlob();
                var arguments = subCommand.Definition().arguments;
                Assert.NotNull(arguments.Select(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }

        public class RealArguments_TestsGroup
        {
            // test if it has proper arguments we defined
        }

        public class GenericExecute_TestsGroup
        {
            private readonly string _path = "a.txt";
            private readonly string _identity = "123";
            private readonly string _url = "https://www.example.com";
            private readonly bool _force = false;
            private readonly string _failMsg = "Cannot convert input object to proper class";
            private readonly Response<BlobContentInfo> _bci = null;

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
                    file = _path,
                    blob = _url,
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
                Assert.Contains(_path, actualExc.Message);
            }
        }


        //
        // main tests
        //

        public class SetBlobExecute_TestsGroup
        {
            // mock Upload method to return success or failure and check SetBlob status

            // setup fake variables, easier that using Any constructs
            private readonly string _path = "a.txt";
            private readonly string _identity = "123";
            private readonly string _url = "https://www.example.com";
            private readonly bool _force= false;
            private readonly Exception _exception = new Exception("testing exception");
            private readonly Response<BlobContentInfo> _bci = null;


            [Fact]
            public void ExecutedWithSuccess()
            {
                // mock with success
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_path, _force).Returns(_bci);
                var subCommand = new SetBlob(blobSubstitute);

                var retValue = subCommand.Execute(_path, _url, _identity, _force);
                Assert.Equal("Success", retValue);

            }

            [Fact]
            public void ExecutedWithFailure_WithIdentity()
            {
                // mock with exception and call with identity
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_path, _force).Throws(_exception);
                var subCommand = new SetBlob(blobSubstitute);

                // it returns testing exception
                var actualExc = Assert.Throws<Exception>(
                    () => subCommand.Execute(_path, _url, _identity, _force)
                );
                Assert.Equal(_exception.Message, actualExc.Message);
            }

            [Fact]
            public void ExecutedWithFailure_NoIdentity()
            {
                // mock with exception and call with no identity
                var blobSubstitute = Substitute.For<IBlobClient>();
                blobSubstitute.Upload(_path, _force).Throws(_exception);
                var subCommand = new SetBlob(blobSubstitute);

                // it returns Azmi exception and testing one as inner
                var actualExc = Assert.Throws<AzmiException>(
                    () => subCommand.Execute(_path, _url, null, _force)
                );
                Assert.Equal(_exception.Message, actualExc.InnerException.Message);

            }
        }


    }
}
