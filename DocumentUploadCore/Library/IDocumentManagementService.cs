using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentUploadCore
{
    public interface IDocumentManagementService
    {
        Task<int> SaveDocument(ManagedDocument documentToSave);
        Task<IReadOnlyList<ManagedDocumentMetadata>> ListDocuments();
        Task DeleteDocument(int documentToDelete);
        Task<ManagedDocument> GetDocument(int documentToRetrieve);
    }
}
