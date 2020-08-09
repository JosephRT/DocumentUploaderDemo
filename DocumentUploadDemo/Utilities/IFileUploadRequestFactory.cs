using Microsoft.AspNetCore.Http;

namespace DocumentUploadDemo.Utilities
{
    public interface IFileUploadRequestFactory
    {
        FileUploadRequest GetFileUploadRequest(HttpRequest request);
    }
}
