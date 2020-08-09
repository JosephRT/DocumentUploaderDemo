using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentUploadApi;
using DocumentUploadApi.Controllers;
using DocumentUploadApi.Utilities;
using DocumentUploadCore.Entities;
using DocumentUploadCore.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace DocumentUploadApiTests
{
    [TestFixture]
    public class FilesControllerTests
    {
        private Mock<IDocumentManagementService> mockDocumentManagementService;
        private Mock<IFileUploadRequestFactory> mockFileUploadRequestFactory;
        private readonly FileUploadSettings fileUploadSettings = new FileUploadSettings {ServerMaxAllowedFileSizeInMb = 1};

        [SetUp]
        public void SetUp()
        {
            mockDocumentManagementService = new Mock<IDocumentManagementService>();
            mockFileUploadRequestFactory = new Mock<IFileUploadRequestFactory>();
        }

        [Test]
        public async Task GetSuccessfullyListsAllDocuments()
        {
            var testDocumentsMetadata = new List<ManagedDocumentMetadata>
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
            mockDocumentManagementService.Setup(d => d.ListDocumentsAsync())
                .ReturnsAsync(testDocumentsMetadata);
            
            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Get();

            Assert.That(result, Is.TypeOf<OkObjectResult>()
                .And.Property("Value").EqualTo(testDocumentsMetadata));
        }

        [Test]
        public async Task IfThereAreNoDocumentsGetReturnsEmptyList()
        {
            mockDocumentManagementService.Setup(d => d.ListDocumentsAsync())
                .ReturnsAsync(new List<ManagedDocumentMetadata>());

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Get();

            Assert.That(result, Is.TypeOf<OkObjectResult>()
                .And.Property(nameof(OkObjectResult.Value)).Empty);
        }

        [Test]
        public async Task GetSpecificDocumentReturnsDocumentsContents()
        {
            var testDocument = new ManagedDocument
            {
                Contents = new byte[1],
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = "txt",
                    Id = 1,
                    Name = "TestFile"
                }
            };
            mockDocumentManagementService.Setup(d => d.GetDocumentAsync(testDocument.Metadata.Id))
                .ReturnsAsync(testDocument);

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Get(testDocument.Metadata.Id);

            Assert.That(result, Is.TypeOf<FileContentResult>()
                .And.Property(nameof(FileContentResult.ContentType)).EqualTo("APPLICATION/octet-stream")
                .And.Property(nameof(FileContentResult.FileContents)).EqualTo(testDocument.Contents)
                .And.Property(nameof(FileContentResult.FileDownloadName)).EqualTo(testDocument.Metadata.FullFileName));
        }

        [Test]
        public async Task GetSpecificDocumentThatDoesNotExistReturnsNotFound()
        {
            mockDocumentManagementService.Setup(d => d.GetDocumentAsync(It.IsAny<int>()))
                .ReturnsAsync((ManagedDocument)null);

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Get(1);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task ValidPostDocumentSucceeds()
        {
            var documentsToSave = new[]
            {
                new ManagedDocument
                {
                    Contents = new byte[1],
                    Metadata = new ManagedDocumentMetadata
                    {
                        Created = DateTime.Now,
                        FileType = "txt",
                        Id = 0,
                        Name = "Test1"
                    }
                },
                new ManagedDocument
                {
                    Contents = new byte[2],
                    Metadata = new ManagedDocumentMetadata
                    {
                        Created = DateTime.Now,
                        FileType = "txt",
                        Id = 0,
                        Name = "Test2"
                    }
                }
            };
            var mockFileUploadRequest = new Mock<FileUploadRequest>();
            mockFileUploadRequest.Setup(f => f.ReadUploadedFiles())
                .ReturnsAsync(documentsToSave);
            mockFileUploadRequestFactory.Setup(f => f.GetFileUploadRequest(It.IsAny<HttpRequest>(), It.Is<FileUploadSettings>(p => p == fileUploadSettings)))
                .Returns(mockFileUploadRequest.Object);
            mockDocumentManagementService.Setup(d => d.SaveDocumentAsync(It.IsAny<ManagedDocument>()));

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Post();

            Assert.That(result, Is.TypeOf<CreatedResult>());
            mockDocumentManagementService.Verify(d => d.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == documentsToSave[0])), Times.Once);
            mockDocumentManagementService.Verify(d => d.SaveDocumentAsync(It.Is<ManagedDocument>(p => p == documentsToSave[1])), Times.Once);
        }

        [Test]
        public async Task InvalidPostDocumentReturnsBadRequest()
        {
            const string expectedExceptionMessage = "File upload failed";
            var mockFileUploadRequest = new Mock<FileUploadRequest>();
            mockFileUploadRequest.Setup(f => f.ReadUploadedFiles())
                .ThrowsAsync(new InvalidFileUploadException(expectedExceptionMessage));
            mockFileUploadRequestFactory.Setup(f => f.GetFileUploadRequest(It.IsAny<HttpRequest>(), It.IsAny<FileUploadSettings>()))
                .Returns(mockFileUploadRequest.Object);

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Post();

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>()
                .And.Property(nameof(BadRequestObjectResult.Value)).EqualTo(expectedExceptionMessage));
            mockDocumentManagementService.Verify(d => d.SaveDocumentAsync(It.IsAny<ManagedDocument>()), Times.Never);
        }

        [Test]
        public async Task DeleteSuccessfullyDeletesDocument()
        {
            const int docIdToDelete = 1;
            mockDocumentManagementService.Setup(d => d.DeleteDocumentAsync(It.Is<int>(p => p == docIdToDelete)));

            var controller = new FilesController(mockDocumentManagementService.Object, mockFileUploadRequestFactory.Object, fileUploadSettings);
            var result = await controller.Delete(docIdToDelete);

            Assert.That(result, Is.TypeOf<OkResult>());
            mockDocumentManagementService.VerifyAll();
        }
    }
}
