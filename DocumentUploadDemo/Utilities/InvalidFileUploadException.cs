using System;

namespace DocumentUploadDemo.Utilities
{
    public class InvalidFileUploadException : Exception
    {
        public InvalidFileUploadException(string message) : base(message)
        { }
    }
}