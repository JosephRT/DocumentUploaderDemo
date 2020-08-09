using DocumentUploadDemo;
using DocumentUploadDemo.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace DocumentUploadApiTests
{
    [TestFixture]
    public class FileUploadRequestFactoryTests
    {
        private readonly FileUploadSettings fileUploadSettings = new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 };

        [Test]
        public void RequestWithMultipartContentCreatesStreamingRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(r => r.ContentType).Returns("multipart/form-data");

            var factory = new FileUploadRequestFactory();
            var createdRequest = factory.GetFileUploadRequest(mockRequest.Object, fileUploadSettings);

            Assert.That(createdRequest, Is.TypeOf<StreamingFileUploadRequest>());
        }

        [Test]
        public void RequestWithoutMultipartContentCreatesPostBodyRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(r => r.ContentType).Returns("text/plain");

            var factory = new FileUploadRequestFactory();
            var createdRequest = factory.GetFileUploadRequest(mockRequest.Object, fileUploadSettings);

            Assert.That(createdRequest, Is.TypeOf<PostBodyFileUploadRequest>());
        }
    }
}
