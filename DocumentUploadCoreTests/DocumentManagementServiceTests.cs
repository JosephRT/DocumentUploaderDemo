using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentUploadCore.Data;
using DocumentUploadCore.Entities;
using DocumentUploadCore.Library;
using Moq;
using NUnit.Framework;

namespace DocumentUploadCoreTests
{
    [TestFixture]
    public class DocumentManagementServiceTests
    {
        private Mock<IDocumentRepository> mockDocumentRepo;


        [SetUp]
        public void SetUp()
        {
            mockDocumentRepo = new Mock<IDocumentRepository>();
        }

        [Test]
        public void ValidDeleteDocumentCallSuccessfullyDeletesDocument()
        {
            const int testDocumentIdToDelete = 1;
            mockDocumentRepo.Setup(mdr => mdr.DeleteDocumentAsync(It.Is<int>(p => p == testDocumentIdToDelete)));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async () => await service.DeleteDocumentAsync(testDocumentIdToDelete), Throws.Nothing);
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public void DeleteDocumentThrowsExceptionIfRepoThrowsException()
        {
            const int testDocumentIdToDelete = 1;
            const string testExceptionMessage = "Error while saving";
            mockDocumentRepo.Setup(mdr => mdr.DeleteDocumentAsync(It.Is<int>(p => p == testDocumentIdToDelete)))
                .ThrowsAsync(new Exception(testExceptionMessage));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async () => await service.DeleteDocumentAsync(testDocumentIdToDelete), Throws.TypeOf<Exception>()
                .With.Message.EqualTo(testExceptionMessage));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public async Task ValidGetDocumentCallReturnsDocument()
        {
            const int testDocumentIdToRetrieve = 1;
            var testDocument = new ManagedDocument
            {
                Contents = new byte[1],
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = testDocumentIdToRetrieve,
                    Name = "TestFile"
                }
            };
            mockDocumentRepo.Setup(mdr => mdr.GetDocumentAsync(It.Is<int>(p => p == testDocumentIdToRetrieve)))
                .ReturnsAsync(testDocument);

            var service = new DocumentManagementService(mockDocumentRepo.Object);
            var retrievedDocument = await service.GetDocumentAsync(testDocumentIdToRetrieve);

            Assert.That(retrievedDocument, Is.EqualTo(testDocument));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public async Task GetDocumentReturnsNullIfThereIsNoDocument()
        {
            const int testDocumentIdToRetrieve = 1;
            mockDocumentRepo.Setup(mdr => mdr.GetDocumentAsync(It.Is<int>(p => p == testDocumentIdToRetrieve)))
                .ReturnsAsync((ManagedDocument)null);

            var service = new DocumentManagementService(mockDocumentRepo.Object);
            var retrievedDocument = await service.GetDocumentAsync(testDocumentIdToRetrieve);

            Assert.That(retrievedDocument, Is.EqualTo(null));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public void GetDocumentThrowsExceptionIfRepoThrowsException()
        {
            const int testDocumentIdToRetrieve = 1;
            const string testExceptionMessage = "Error while retrieving";
            mockDocumentRepo.Setup(mdr => mdr.GetDocumentAsync(It.Is<int>(p => p == testDocumentIdToRetrieve)))
                .ThrowsAsync(new Exception(testExceptionMessage));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async () => await service.GetDocumentAsync(testDocumentIdToRetrieve), Throws.TypeOf<Exception>()
                .With.Message.EqualTo(testExceptionMessage));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public async Task ValidListDocumentsCallReturnsAllDocuments()
        {
            var testDocuments = new List<ManagedDocumentMetadata>
            {
                new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = 1,
                    Name = "Test1"
                },
                new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = 2,
                    Name = "Test2"
                }
            };
            mockDocumentRepo.Setup(mdr => mdr.ListDocumentsAsync())
                .ReturnsAsync(testDocuments);

            var service = new DocumentManagementService(mockDocumentRepo.Object);
            var retrievedDocuments = await service.ListDocumentsAsync();

            Assert.That(retrievedDocuments, Is.EquivalentTo(testDocuments));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public async Task ValidListDocumentCallsReturnsEmptyListIfThereAreNoDocuments()
        {
            mockDocumentRepo.Setup(mdr => mdr.ListDocumentsAsync())
                .ReturnsAsync(new List<ManagedDocumentMetadata>());

            var service = new DocumentManagementService(mockDocumentRepo.Object);
            var retrievedDocuments = await service.ListDocumentsAsync();

            Assert.That(retrievedDocuments, Is.Empty);
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public void ListDocumentThrowsExceptionIfRepoThrowsException()
        {
            const string testExceptionMessage = "Error while retrieving";
            mockDocumentRepo.Setup(mdr => mdr.ListDocumentsAsync())
                .ThrowsAsync(new Exception(testExceptionMessage));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async () => await service.ListDocumentsAsync(), Throws.TypeOf<Exception>()
                .With.Message.EqualTo(testExceptionMessage));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public async Task ValidSaveDocumentCallSavesDocument()
        {
            const int idToReturn = 1;
            var managedDocumentToSave = new ManagedDocument
            {
                Contents = new byte[1],
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = 0,
                    Name = "TestFile"
                }
            };
            mockDocumentRepo
                .Setup(mdr => mdr.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == managedDocumentToSave)))
                .ReturnsAsync(idToReturn);

            var service = new DocumentManagementService(mockDocumentRepo.Object);
            var newDocumentId = await service.SaveDocumentAsync(managedDocumentToSave);

            Assert.That(newDocumentId, Is.EqualTo(idToReturn));
            mockDocumentRepo.VerifyAll();
        }

        [Test]
        public void SavingDocumentWithInvalidFileExtensionFails()
        {
            var managedDocumentToSave = new ManagedDocument
            {
                Contents = new byte[1],
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "pdf",
                    Id = 0,
                    Name = "TestFile"
                }
            };
            mockDocumentRepo
                .Setup(mdr => mdr.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == managedDocumentToSave)));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async() => await service.SaveDocumentAsync(managedDocumentToSave), Throws.TypeOf<InvalidDataException>()
                .And.Message.EqualTo($"Invalid file extension \"{managedDocumentToSave.Metadata.FileType}\".  Allowed file extensions: txt"));
            mockDocumentRepo.Verify(mdr =>
                mdr.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == managedDocumentToSave)), Times.Never);
        }

        [Test]
        public void SaveDocumentThrowsExceptionIfRepoThrowsException()
        {
            var managedDocumentToSave = new ManagedDocument
            {
                Contents = new byte[1],
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = 0,
                    Name = "TestFile"
                }
            };
            const string testExceptionMessage = "Error while retrieving";
            mockDocumentRepo.Setup(mdr => mdr.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == managedDocumentToSave)))
                .ThrowsAsync(new Exception(testExceptionMessage));

            var service = new DocumentManagementService(mockDocumentRepo.Object);

            Assert.That(async () => await service.SaveDocumentAsync(managedDocumentToSave), Throws.TypeOf<Exception>()
                .With.Message.EqualTo(testExceptionMessage));
            mockDocumentRepo.VerifyAll();
        }
    }
}
