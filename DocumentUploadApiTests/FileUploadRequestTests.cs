using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

            var result = await FileUploadRequest.ProcessStreamContents(testStream);

            Assert.That(result, Is.EqualTo(testStream.ToArray()));
        }

        [Test]
        public void EmptyStreamReturnsException()
        {
            var testStream = new MemoryStream();

            Assert.That(async () => await FileUploadRequest.ProcessStreamContents(testStream), Throws.TypeOf<InvalidFileUploadException>()
                .And.Message.EqualTo("The file is empty"));
        }

        [Test]
        public void NullStreamReturnsException()
        {
            Assert.That(async () => await FileUploadRequest.ProcessStreamContents(null), Throws.TypeOf<Exception>()
                .And.Message.EqualTo("File upload failed"));
        }
    }
}
