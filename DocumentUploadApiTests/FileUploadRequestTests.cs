using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;
using DocumentUploadDemo;
using DocumentUploadDemo.Utilities;
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
        public void EmptyStreamReturnsException()
        {
            var testStream = new MemoryStream();

            var request = new TestFileUploadRequest(new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 });
            Assert.That(async () => await request.ProcessStreamContents(testStream), Throws.TypeOf<InvalidFileUploadException>()
                .And.Message.EqualTo("The file is empty"));
        }

        [Test]
        public void NullStreamReturnsException()
        {
            var request = new TestFileUploadRequest(new FileUploadSettings { ServerMaxAllowedFileSizeInMb = 1 });
            Assert.That(async () => await request.ProcessStreamContents(null), Throws.TypeOf<Exception>()
                .And.Message.EqualTo("File upload failed"));
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
