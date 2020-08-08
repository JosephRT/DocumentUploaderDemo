using Microsoft.AspNetCore.Http;

namespace DocumentUploadDemo.Utilities
{
    public class FileUploadRequestFactory : IFileUploadRequestFactory
    {
        public IFileUploadRequest GetFileUploadRequest(HttpRequest request)
        {
            if (request.ContentType.Contains("multipart/form-data"))
            {
                return new StreamingFileUploadRequest(request);
            }
            
            return new PostBodyFileUploadRequest(request);
        }
    }
}