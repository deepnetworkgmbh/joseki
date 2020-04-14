using webapp.Audits;

namespace webapp.BlobStorage
{
    /// <summary>
    /// Describes a single root-level container object of Blob Storage.
    /// </summary>
    public class ScannerContainer
    {
        /// <summary>
        /// Represents empty container.
        /// </summary>
        public static readonly ScannerContainer Empty = new ScannerContainer(string.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerContainer"/> class.
        /// </summary>
        /// <param name="name">Scanner container name.</param>
        public ScannerContainer(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Unique per Blob Storage container name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Scanner metadata.
        /// </summary>
        public ScannerMetadata Metadata { get; set; }

        /// <summary>
        /// Relative path to metadata file.
        /// </summary>
        public string MetadataFilePath => $"{this.Name}/{this.Name}";
    }
}