using System;

namespace DocumentUploadApi.Utilities
{
    public class InvalidFileUploadException : Exception
    {
        public InvalidFileUploadException(string message) : base(message)
        { }
    }
}