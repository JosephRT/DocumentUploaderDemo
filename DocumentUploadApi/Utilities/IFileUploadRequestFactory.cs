using Microsoft.AspNetCore.Http;

namespace DocumentUploadApi.Utilities
{
    public interface IFileUploadRequestFactory
    {
        FileUploadRequest GetFileUploadRequest(HttpRequest request, FileUploadSettings settings);
    }
}
