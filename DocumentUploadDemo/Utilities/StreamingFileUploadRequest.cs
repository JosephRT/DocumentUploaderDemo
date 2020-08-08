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

namespace DocumentUploadDemo.Utilities
{
    public class StreamingFileUploadRequest : IFileUploadRequest
    {
        private readonly DateTime createdOn;
        private readonly HttpRequest request;
        private static readonly int BoundaryLengthLimit = new FormOptions().MultipartBoundaryLengthLimit;
        private const long FileSizeLimit = 10_000_000_000_000_000;
        private static readonly string[] PermittedExtensions = { ".txt" };


        public StreamingFileUploadRequest(HttpRequest request)
        {
            createdOn = DateTime.Now;
            this.request = request;
        }


        public async Task<ManagedDocument[]> ReadUploadedFiles()
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
                                FileType = Path.GetExtension(fullFileName).ToLowerInvariant(),
                                Name = Path.GetFileNameWithoutExtension(fullFileName)
                            }
                        };
                    }

                    var streamedFileContent = await ProcessStreamedFile(multipartFormSection, contentDisposition, FileSizeLimit);
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
            if (boundary.Length > BoundaryLengthLimit)
            {
                throw new InvalidFileUploadException($"Multipart boundary length limit {BoundaryLengthLimit} exceeded.");
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

        private static async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition, long sizeLimit)
        {
            try
            {
                await using var memoryStream = new MemoryStream();
                await section.Body.CopyToAsync(memoryStream);

                if (memoryStream.Length == 0)
                {
                    throw new InvalidFileUploadException("The file is empty");
                }

                if (memoryStream.Length > sizeLimit)
                {
                    var megabyteSizeLimit = sizeLimit / 1048576;
                    throw new InvalidFileUploadException($"The file exceeds {megabyteSizeLimit:N1} MB.");
                }

                if (string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    throw new InvalidFileUploadException("File name and extension missing");
                }

                var fileExtension = Path.GetExtension(contentDisposition.FileName.Value)?.ToLowerInvariant();
                if (!PermittedExtensions.Contains(fileExtension))
                {
                    throw new InvalidFileUploadException($"Invalid file extension \"{fileExtension}\" disallowed; ");
                }

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("File upload failed", ex);
            }
        }
    }
}
