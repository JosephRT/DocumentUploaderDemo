using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ValidateDocument(documentToSave);

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

        private static void ValidateDocument(ManagedDocument documentToSave)
        {
            var allowedFileExtensions = new [] {"txt"};

            if (allowedFileExtensions.Contains(documentToSave.Metadata.FileType))
            {
                throw new InvalidDataException($"Invalid file extension \"{documentToSave.Metadata.FileType}\".  Allowed file extensions: {string.Join(",", allowedFileExtensions)}");
            }
        }
    }
}