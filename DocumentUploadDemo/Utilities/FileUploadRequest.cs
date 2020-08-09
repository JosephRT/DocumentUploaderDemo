using System;
using System.IO;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;

namespace DocumentUploadDemo.Utilities
{
    public abstract class FileUploadRequest
    {
        public abstract Task<ManagedDocument[]> ReadUploadedFiles();

        internal static async Task<byte[]> ProcessStreamContents(Stream bodyStream)
        {
            try
            {
                await using var memoryStream = new MemoryStream();
                await bodyStream.CopyToAsync(memoryStream);

                if (memoryStream.Length == 0)
                {
                    throw new InvalidFileUploadException("The file is empty");
                }

                const long fileSizeLimitInBytes = 15_000_000;
                if (memoryStream.Length > fileSizeLimitInBytes)
                {
                    const long megabyteSizeLimit = fileSizeLimitInBytes / 1_048_576;
                    throw new InvalidFileUploadException($"The file exceeds {megabyteSizeLimit:N1} MB.");
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