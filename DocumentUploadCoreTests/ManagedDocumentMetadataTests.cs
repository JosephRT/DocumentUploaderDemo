using DocumentUploadCore.Entities;
using NUnit.Framework;

namespace DocumentUploadCoreTests
{
    [TestFixture]
    public class ManagedDocumentMetadataTests
    {
        [Test]
        public void FullFileNameReturnsCorrectFileName()
        {
            var testDocument = new ManagedDocumentMetadata
            {
                FileType = "txt",
                Name = "TestFile"
            };

            Assert.That(testDocument.FullFileName, Is.EqualTo($"{testDocument.Name}.{testDocument.FileType}"));
        }
    }
}
