namespace webapp.BlobStorage
{
    /// <summary>
    /// Describes a single audit folder within a scanner container.
    /// </summary>
    public class AuditBlob
    {
        /// <summary>
        /// Unique audit blob metadata file name (including path within a container).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parent scanner container.
        /// </summary>
        public ScannerContainer ParentContainer { get; set; }
    }
}