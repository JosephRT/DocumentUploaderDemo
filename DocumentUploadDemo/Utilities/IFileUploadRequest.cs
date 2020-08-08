using System.Threading.Tasks;
using DocumentUploadCore.Entities;

namespace DocumentUploadDemo.Utilities
{
    public interface IFileUploadRequest
    {
        Task<ManagedDocument[]> ReadUploadedFiles();
    }
}