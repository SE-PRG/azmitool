using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using azmi_main;
using System.Linq;
using NSubstitute;
using Azure.Storage.Blobs.Models;
using Azure;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

namespace azmi_tests
{
    public class ListBlobs_Tests
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
                var subCommand = new ListBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.identity.name));
            }

            [Fact]
            public void VerboseArgument()
            {
                var subCommand = new ListBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.NotEmpty(arguments.Where(a => a.name == SharedAzmiArguments.verbose.name));
            }
        }


        public class RealArguments_TestsGroup
        {
            // test if it has real arguments we defined, prevents accidental argument removal
            [Theory]
            [InlineData("container")]
            [InlineData("prefix")]
            [InlineData("exclude")]
            public void TestRealArguments(string argName)
            {
                // Arrange
                var subCommand = new ListBlobs();
                // Act
                var arguments = subCommand.Definition().arguments;
                // Assert
                Assert.NotEmpty(arguments.Where(a => a.name == argName));
            }

            [Fact]
            public void NoFakeArgument()
            {
                var subCommand = new ListBlobs();
                var arguments = subCommand.Definition().arguments;
                Assert.Empty(arguments.Where(a => a.name == "fakeArgument"));
            }
        }


        public class GenericExecute_TestsGroup
        {
            private readonly Uri _anyValidURL = new Uri("https://www.example.com");
            private readonly string _failMsg = "Cannot convert input object to proper class";
            private readonly Exception _testException = new Exception("testing exception");


            // test if it will call real execute method
            [Fact]
            public void FailsWithNonExistingProperty()
            {
                var obj = new { nonExistingProperty = 1 };
                var subCommand = new ListBlobs();

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
                var obj = new ListBlobs.AzmiArgumentsClass
                {
                    container = _anyValidURL,
                    prefix = null,
                    exclude = null,
                    identity = null
                };

                var containerSubstitute = Substitute.For<IContainerClient>();
                containerSubstitute.GetBlobs(null).Throws(_testException);
                var subCommand = new ListBlobs(containerSubstitute);

                // Act & Assert
                var actualExc = Assert.Throws<AzmiException>(
                    () => subCommand.Execute(obj)
                );
                Assert.NotEqual(_failMsg, actualExc.Message);
                Assert.Equal(_testException.Message, actualExc.InnerException.Message);
            }
        }


        //
        // main tests
        //

        public class ListBlobsExecute_TestsGroup
        {

            private readonly Uri _anyValidURL = new Uri("https://www.example.com");
            private static readonly Pageable<BlobItem> _nullBlobItems = null;
            //private static readonly BlobItem emptyBlob = new BlobItem() { Name = "testBlob" };
            //private static readonly Pageable<BlobItem> _zeroBlobs = new MyPageable(new List<Page<BlobItem>>() { });
            //private static readonly Pageable<BlobItem> _oneBlob = new MyPageable(new List<Page<BlobItem>>() { null });
            //private static readonly Pageable<BlobItem> _twoBlobs = new MyPageable(new List<Page<BlobItem>>() { null, null });


            //class MyPageable : Pageable<BlobItem>
            //{
            //    private List<Page<BlobItem>> _blobs;

            //    public override IEnumerable<Page<BlobItem>> AsPages(string continuationToken = null, int? pageSizeHint = null)
            //    {
            //        return _blobs.AsEnumerable();
            //    }

            //    public MyPageable(List<Page<BlobItem>> inputBlobs)
            //    {
            //        _blobs = inputBlobs;
            //    }
            //}


            // returns null
            [Fact]
            public void ReturnsNull()
            {
                // Arrange
                var containerSubstitute = Substitute.For<IContainerClient>();
                var _zeroBlobs = Substitute.For<Pageable<BlobItem>>();
                containerSubstitute.GetBlobs(prefix: null).Returns(_zeroBlobs);
                var subCommand = new ListBlobs(containerSubstitute);
                // Act
                var retValue = subCommand.Execute(_anyValidURL, prefix: null);
                // Assert
                Assert.Null(retValue);
            }

            // returns one object
            //[Fact]
            //public void ReturnsOneBlob()
            //{
            //    // Arrange
            //    var containerSubstitute = Substitute.For<IContainerClient>();
            //    var _oneBlob = Substitute.For<Pageable<BlobItem>>();
            //    //_oneBlob.ReturnsForAll(null, <Pageable<BlobItem>>);
            //    // _oneBlob.Append(null);
            //    //_oneBlob.GetEnumerator().Returns((IEnumerator<BlobItem>)null);
            //    var _fakeBlob = Substitute.For<BlobItem>();
            //    _fakeBlob.Name.Returns("myBlob");
            //    IEnumerator<BlobItem> _fakeEnumerator = ((IEnumerable<BlobItem>)(new[] { _fakeBlob })).GetEnumerator();
            //    _oneBlob.GetEnumerator().Returns(_fakeEnumerator); ;
            //    containerSubstitute.GetBlobs(prefix: null).Returns(_oneBlob);
            //    var subCommand = new ListBlobs(containerSubstitute);
            //    // Act & Assert
            //    var retValue = subCommand.Execute(_anyValidURL, prefix: null);
            //    Assert.Single(retValue);
            //    Assert.Equal("myBlob", retValue.Single());
            //}

            // returns two objects
            // use exclude
            // throw an exception

        }
    }
}
