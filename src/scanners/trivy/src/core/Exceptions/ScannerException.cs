using System;

namespace core.Exceptions
{
    [Serializable]
    public class ScannerException : Exception
    {
        public ScannerException(string message)
            : base(message)
        {
        }

        public ScannerException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}