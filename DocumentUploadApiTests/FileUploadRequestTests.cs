using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DocumentUploadApi;
using DocumentUploadApi.Utilities;
using DocumentUploadCore.Entities;
using NUnit.Framework;

namespace DocumentUploadApiTests
{
    [TestFixture]
    public class FileUploadRequestTests
    {
        [Test]
        public async Task ValidStreamIsReadIntoByteArray()
        {
            var testStream = new MemoryStream(Encoding.UTF8.GetBytes("TestStreamContents"));

            var request = new TestFileUploadRequest(new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 });
            var result = await request.ProcessStreamContents(testStream);

            Assert.That(result, Is.EqualTo(testStream.ToArray()));
        }

        [Test]
        public void EmptyStreamThrowsException()
        {
            var testStream = new MemoryStream();

            var request = new TestFileUploadRequest(new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 });
            Assert.That(async () => await request.ProcessStreamContents(testStream), Throws.TypeOf<InvalidFileUploadException>()
                .And.Message.EqualTo("The file is empty"));
        }

        [Test]
        public void NullStreamThrowsException()
        {
            var request = new TestFileUploadRequest(new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 });
            Assert.That(async () => await request.ProcessStreamContents(null), Throws.TypeOf<Exception>()
                .And.Message.EqualTo("File upload failed"));
        }

        [Test]
        public void StreamTooLargeThrowsException()
        {
            var testStream = new MemoryStream(Encoding.UTF8.GetBytes("TestStreamContents"));
            var testSettings = new FileUploadSettings {ServerMaxAllowedFileSizeInMb = 0};

            var request = new TestFileUploadRequest(testSettings);
            Assert.That(async () => await request.ProcessStreamContents(testStream), Throws.TypeOf<InvalidFileUploadException>()
                .And.Message.EqualTo($"The file exceeds {testSettings.ServerMaxAllowedFileSizeInMb} MB."));
        }


        private class TestFileUploadRequest : FileUploadRequest
        {
            public TestFileUploadRequest(FileUploadSettings settings) : base(settings) { }

            public override Task<ManagedDocument[]> ReadUploadedFiles()
            {
                throw new NotImplementedException();
            }
        }
    }
}
