using System.Threading.Tasks;
using DocumentUploadCore;

namespace DocumentUploadDemo.Utilities
{
    public interface IFileUploadRequest
    {
        Task<ManagedDocument[]> ReadUploadedFiles();
    }
}