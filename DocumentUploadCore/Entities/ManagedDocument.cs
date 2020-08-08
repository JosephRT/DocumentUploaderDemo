namespace DocumentUploadCore
{
    public class ManagedDocument
    {
        public byte[] Contents { get; set; }
        public ManagedDocumentMetadata Metadata { get; set; }
    }
}