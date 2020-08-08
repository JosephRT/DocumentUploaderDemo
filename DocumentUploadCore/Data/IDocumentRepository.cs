using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentUploadCore
{
    public interface IDocumentRepository
    {
        Task DeleteDocument(int documentToDelete);
        Task<ManagedDocument> GetDocument(int documentToRetrieve);
        Task<IReadOnlyList<ManagedDocumentMetadata>> ListDocuments();
        Task<int> SaveDocument(ManagedDocument documentToSave);
    }
}
