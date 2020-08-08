using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentUploadCore.Data;
using DocumentUploadCore.Entities;

namespace DocumentUploadCore.Library
{
    public class DocumentManagementService : IDocumentManagementService
    {
        private readonly IDocumentRepository documentRepository;


        public DocumentManagementService(IDocumentRepository documentRepository)
        {
            this.documentRepository = documentRepository;
        }


        public Task<int> SaveDocumentAsync(ManagedDocument documentToSave)
        {
            return documentRepository.SaveDocumentAsync(documentToSave);
        }

        public Task<IList<ManagedDocumentMetadata>> ListDocumentsAsync()
        {
            return documentRepository.ListDocumentsAsync();
        }

        public Task DeleteDocumentAsync(int documentToDelete)
        {
            return documentRepository.DeleteDocumentAsync(documentToDelete);
        }

        public Task<ManagedDocument> GetDocumentAsync(int documentToRetrieve)
        {
            return documentRepository.GetDocumentAsync(documentToRetrieve);
        }
    }
}