using Microsoft.AspNetCore.Http;

namespace DocumentUploadApi.Utilities
{
    public class FileUploadRequestFactory : IFileUploadRequestFactory
    {
        public FileUploadRequest GetFileUploadRequest(HttpRequest request, FileUploadSettings settings)
        {
            if (request.ContentType.Contains("multipart/form-data"))
            {
                return new StreamingFileUploadRequest(request, settings);
            }
            
            return new PostBodyFileUploadRequest(request, settings);
        }
    }
}