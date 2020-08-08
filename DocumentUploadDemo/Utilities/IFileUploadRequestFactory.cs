using Microsoft.AspNetCore.Http;

namespace DocumentUploadDemo.Utilities
{
    public interface IFileUploadRequestFactory
    {
        IFileUploadRequest GetFileUploadRequest(HttpRequest request);
    }
}
