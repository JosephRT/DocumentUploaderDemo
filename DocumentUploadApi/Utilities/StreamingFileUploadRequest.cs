using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace DocumentUploadApi.Utilities
{
    public class StreamingFileUploadRequest : FileUploadRequest
    {
        private readonly DateTime createdOn;
        private readonly HttpRequest request;


        public StreamingFileUploadRequest(HttpRequest request, FileUploadSettings settings) : base(settings)
        {
            createdOn = DateTime.Now;
            this.request = request;
        }


        public override async Task<ManagedDocument[]> ReadUploadedFiles()
        {
            var managedDocuments = new Dictionary<string, ManagedDocument>();
            var multipartFormBoundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType));
            var multipartFormReader = new MultipartReader(multipartFormBoundary, request.Body);
            var multipartFormSection = await multipartFormReader.ReadNextSectionAsync();

            while (multipartFormSection != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(multipartFormSection.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    ValidateFileContentDispositionHeader(contentDisposition);

                    var fullFileName = contentDisposition.FileName.Value;
                    ManagedDocument currentManagedDocument;

                    if (managedDocuments.ContainsKey(fullFileName))
                    {
                        currentManagedDocument = managedDocuments[fullFileName];
                    }
                    else
                    {
                        currentManagedDocument = new ManagedDocument
                        {
                            Metadata = new ManagedDocumentMetadata
                            {
                                Created = createdOn,
                                FileType = Path.GetExtension(fullFileName).ToLowerInvariant().Replace(".", string.Empty),
                                Name = Path.GetFileNameWithoutExtension(fullFileName)
                            }
                        };
                    }

                    var streamedFileContent = await ProcessStreamedFile(multipartFormSection, contentDisposition);
                    // TODO: Need to append any current contents to the new contents
                    currentManagedDocument.Contents = streamedFileContent;

                    managedDocuments[fullFileName] = currentManagedDocument;
                }

                multipartFormSection = await multipartFormReader.ReadNextSectionAsync();
            }

            return managedDocuments.Select(md => md.Value).ToArray();
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidFileUploadException("Missing content-type boundary.");
            }

            // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
            var boundaryLengthLimit = new FormOptions().MultipartBoundaryLengthLimit;
            if (boundary.Length > boundaryLengthLimit)
            {
                throw new InvalidFileUploadException($"Multipart boundary length limit {boundaryLengthLimit} exceeded.");
            }

            return boundary;
        }

        private static void ValidateFileContentDispositionHeader(ContentDispositionHeaderValue contentDisposition)
        {
            if (contentDisposition == null)
            {
                throw new InvalidFileUploadException("Attempted to read contentDisposition, but it was null");
            }

            var dispositionTypeIsWrong = !contentDisposition.DispositionType.Equals("form-data");
            if (dispositionTypeIsWrong)
            {
                throw new InvalidFileUploadException($"Content disposition type is \"{contentDisposition.DispositionType}\"; expected \"form-data\"");
            }

            var missingFileName = string.IsNullOrEmpty(contentDisposition.FileName.Value) && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
            if (missingFileName)
            {
                throw new InvalidFileUploadException("Content disposition's filename is missing");
            }
        }

        private async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition)
        {
            var sectionContents = await ProcessStreamContents(section.Body);

            if (string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                throw new InvalidFileUploadException("File name and extension missing");
            }

            return sectionContents;
        }
    }
}
