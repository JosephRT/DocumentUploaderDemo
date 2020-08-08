using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentUploadCore.Entities;

namespace DocumentUploadCore.Data
{
    public interface IDocumentRepository
    {
        Task DeleteDocumentAsync(int documentToDelete);
        Task<ManagedDocument> GetDocumentAsync(int documentToRetrieve);
        Task<IList<ManagedDocumentMetadata>> ListDocumentsAsync();
        Task<int> SaveDocumentAsync(ManagedDocument documentToSave);
    }
}
