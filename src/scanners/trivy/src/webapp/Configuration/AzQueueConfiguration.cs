namespace webapp.Configuration
{
    /// <summary>
    /// Represents Azure Storage Queue configuration.
    /// </summary>
    public class AzQueueConfiguration : IQueueConfiguration
    {
        /// <summary>
        /// Base Azure Storage Queue URL.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Image Scan Request queue name.
        /// </summary>
        public string MainQueue { get; set; }

        /// <summary>
        /// Sas token to process image-scan requests.
        /// </summary>
        public string MainQueueSas { get; set; }

        /// <summary>
        /// Image Scan Request quarantine queue name.
        /// </summary>
        public string QuarantineQueue { get; set; }

        /// <summary>
        /// Sas token to insert messages to quarantine queue.
        /// </summary>
        public string QuarantineQueueSas { get; set; }
    }
}