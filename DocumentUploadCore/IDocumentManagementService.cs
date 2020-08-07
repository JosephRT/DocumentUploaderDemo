using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentUploadCore
{
    interface IDocumentManagementService
    {
        Task<string> SaveDocument(ManagedDocument documentToSave);
        Task<IReadOnlyList<ManagedDocumentMetadata>> ListDocuments();
        Task DeleteDocument(string documentToDelete);
        Task<ManagedDocument> GetDocument(string documentToRetrieve);
    }

    public class DocumentManagementService : IDocumentManagementService
    {
        public Task<string> SaveDocument(ManagedDocument documentToSave)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyList<ManagedDocumentMetadata>> ListDocuments()
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteDocument(string documentToDelete)
        {
            throw new System.NotImplementedException();
        }

        public Task<ManagedDocument> GetDocument(string documentToRetrieve)
        {
            throw new System.NotImplementedException();
        }
    }
}
