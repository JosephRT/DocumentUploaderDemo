using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentUploadCore
{
    public class DocumentManagementService : IDocumentManagementService
    {
        private readonly IDocumentRepository documentRepository;


        public DocumentManagementService(IDocumentRepository documentRepository)
        {
            this.documentRepository = documentRepository;
        }


        public Task<int> SaveDocument(ManagedDocument documentToSave)
        {
            return documentRepository.SaveDocument(documentToSave);
        }

        public Task<IReadOnlyList<ManagedDocumentMetadata>> ListDocuments()
        {
            return documentRepository.ListDocuments();
        }

        public Task DeleteDocument(int documentToDelete)
        {
            return documentRepository.DeleteDocument(documentToDelete);
        }

        public Task<ManagedDocument> GetDocument(int documentToRetrieve)
        {
            return documentRepository.GetDocument(documentToRetrieve);
        }
    }
}