using System;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;
using Microsoft.AspNetCore.Http;

namespace DocumentUploadApi.Utilities
{
    public class PostBodyFileUploadRequest : FileUploadRequest
    {
        private readonly HttpRequest request;


        public PostBodyFileUploadRequest(HttpRequest request, FileUploadSettings settings) : base(settings)
        {
            this.request = request;
        }


        public override async Task<ManagedDocument[]> ReadUploadedFiles()
        {
            ValidateFileUpload();

            var fileName = request.Query["fileName"];
            var fileContents = await ProcessStreamContents(request.Body);

            var readDocument = new ManagedDocument
            {
                Contents = fileContents,
                Metadata = new ManagedDocumentMetadata
                {
                    Created = DateTime.Now,
                    FileType = ResolveFileExtensionFromContentType(),
                    Name = fileName
                }
            };

            return new [] { readDocument };
        }

        private void ValidateFileUpload()
        {
            var didNotSpecifyFileName = string.IsNullOrEmpty(request.Query["fileName"]);
            if (didNotSpecifyFileName)
            {
                throw new InvalidFileUploadException("\"fileName\" query param must be provided when POSTing a binary file");
            }
        }

        private string ResolveFileExtensionFromContentType()
        {
            return request.ContentType switch
            {
                "text/plain" => "txt",
                _ => ""
            };
        }
    }
}
