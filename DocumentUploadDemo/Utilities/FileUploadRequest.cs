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
        public abstract Task<ManagedDocument[]> ReadUploadedFiles();

        
        internal static async Task<byte[]> ProcessStreamContents(Stream bodyStream)
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

            const long fileSizeLimitInBytes = 15_000_000;
            if (memoryStream.Length > fileSizeLimitInBytes)
            {
                const long megabyteSizeLimit = fileSizeLimitInBytes / 1_000_000;
                throw new InvalidFileUploadException($"The file exceeds {megabyteSizeLimit:N1} MB.");
            }

            return memoryStream.ToArray();
        }
    }
}