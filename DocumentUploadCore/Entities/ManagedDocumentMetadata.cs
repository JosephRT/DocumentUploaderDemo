using System;

namespace DocumentUploadCore.Entities
{
    public class ManagedDocumentMetadata
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string FileType { get; set; }
        public string FullFileName => $"{Name}.{FileType}";
        public string Name { get; set; }
    }
}