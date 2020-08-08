using System;
using System.IO;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;
using Microsoft.AspNetCore.Http;

namespace DocumentUploadDemo.Utilities
{
    public class PostBodyFileUploadRequest : IFileUploadRequest
    {
        private readonly HttpRequest request;


        public PostBodyFileUploadRequest(HttpRequest request)
        {
            this.request = request;
        }


        public async Task<ManagedDocument[]> ReadUploadedFiles()
        {
            ValidateFileUpload();

            var fileName = request.Query["fileName"];
            await using var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);

            var readDocument = new ManagedDocument
            {
                Contents = memoryStream.ToArray(),
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = ".txt",
                    Name = fileName
                }
            };

            return new [] { readDocument };
        }

        private void ValidateFileUpload()
        {
            var uploadedFileTypeIsDisallowed = request.ContentType != "text/plain";
            if (uploadedFileTypeIsDisallowed)
            {
                throw new InvalidFileUploadException("Invalid file type uploaded.");
            }

            var didNotSpecifyFileName = string.IsNullOrEmpty(request.Query["fileName"]);
            if (didNotSpecifyFileName)
            {
                throw new InvalidFileUploadException("\"fileName\" query param must be provided when POSTing a binary file");
            }
        }
    }
}
