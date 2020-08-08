using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;

namespace DocumentUploadCore.Library
{
    public interface IDocumentManagementService
    {
        Task<int> SaveDocumentAsync(ManagedDocument documentToSave);
        Task<IList<ManagedDocumentMetadata>> ListDocumentsAsync();
        Task DeleteDocumentAsync(int documentToDelete);
        Task<ManagedDocument> GetDocumentAsync(int documentToRetrieve);
    }
}
