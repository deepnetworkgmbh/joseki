namespace webapp.Models
{
    /// <summary>
    /// The image scan request details.
    /// </summary>
    public class ImageScanRequestMessage
    {
        /// <summary>
        /// Message headers: when the message was created, what is the version of payload, etc.
        /// </summary>
        public MessageHeaders Headers { get; set; }

        /// <summary>
        /// Image Scan Request details: image tag, etc.
        /// </summary>
        public ImageScanRequestPayload Payload { get; set; }
    }

    /// <summary>
    /// Message headers: when the message was created, what is the version of payload, etc.
    /// </summary>
    public class MessageHeaders
    {
        /// <summary>
        /// Unix epoch time, when the scan was requested.
        /// </summary>
        public long CreationTime { get; set; }

        /// <summary>
        /// The version of message payload.
        /// </summary>
        public string PayloadVersion { get; set; }
    }

    /// <summary>
    /// The Image Scan Request message payload.
    /// </summary>
    public class ImageScanRequestPayload
    {
        /// <summary>
        /// The complete image name including registry, and tag.
        /// </summary>
        public string ImageFullName { get; set; }
    }
}