using System;

namespace DocumentUploadCore.Entities
{
    public class ManagedDocumentMetadata
    {
        public string Id { get; set; } = "";
        public DateTime Created { get; set; }
        public string FileType { get; set; }
        public string Name { get; set; }
    }
}