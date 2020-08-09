using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;

[assembly:InternalsVisibleTo("DocumentUploadApiTests")]
namespace DocumentUploadDemo.Utilities
{
    public abstract class FileUploadRequest
    {
        private readonly FileUploadSettings settings;
        public abstract Task<ManagedDocument[]> ReadUploadedFiles();

        protected FileUploadRequest() { }

        protected FileUploadRequest(FileUploadSettings settings)
        {
            this.settings = settings;
        }

        
        internal async Task<byte[]> ProcessStreamContents(Stream bodyStream)
        {
            await using var memoryStream = new MemoryStream();

            try
            {
                await bodyStream.CopyToAsync(memoryStream);
            }
            catch (Exception ex)
            {
                throw new Exception("File upload failed", ex);
            }

            if (memoryStream.Length == 0)
            {
                throw new InvalidFileUploadException("The file is empty");
            }

            var fileSizeLimitInBytes = settings.ServerMaxAllowedFileSizeInMb * 1_000_000;
            if (memoryStream.Length > fileSizeLimitInBytes)
            {
                throw new InvalidFileUploadException($"The file exceeds {settings.ServerMaxAllowedFileSizeInMb} MB.");
            }

            return memoryStream.ToArray();
        }
    }
}