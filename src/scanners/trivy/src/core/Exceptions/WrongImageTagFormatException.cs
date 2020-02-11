using System;

namespace core.Exceptions
{
    [Serializable]
    public class WrongImageTagFormatException : ScannerException
    {
        public WrongImageTagFormatException(string imageTag)
            : base($"{imageTag} is in wrong format")
        {
            this.ImageTag = imageTag;
        }

        public string ImageTag { get; set; }
    }
}