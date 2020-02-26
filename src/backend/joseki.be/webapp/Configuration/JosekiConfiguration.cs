namespace webapp.Configuration
{
    /// <summary>
    /// Joseki Backend service configuration.
    /// </summary>
    public class JosekiConfiguration
    {
        /// <summary>
        /// Database related configuration.
        /// </summary>
        public DatabaseConfig Database { get; set; }

        /// <summary>
        /// Aggregated Cache related configuration.
        /// </summary>
        public CacheConfig Cache { get; set; }

        /// <summary>
        /// Azure Blob related configuration.
        /// </summary>
        public AzureBlobConfig AzureBlob { get; set; }

        /// <summary>
        /// Aggregated Watchmen configs.
        /// </summary>
        public Watchmen Watchmen { get; set; }

        /// <summary>
        /// Azure Storage Queue related configuration.
        /// </summary>
        public AzureQueueConfig AzureQueue { get; set; }
    }

    /// <summary>
    /// Database related configuration.
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// MSSQL server connection string with place-holders fow username and password.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// MSSQL database username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// MSSQL database password.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Cache related configurations.
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// How often Polaris Check data should be updated.
        /// The measurement is in days.
        /// </summary>
        public int PolarisCheckTtl { get; set; } = 7;

        /// <summary>
        /// How often Azure Check data should be updated.
        /// The measurement is in days.
        /// </summary>
        public int AzureCheckTtl { get; set; } = 7;

        /// <summary>
        /// How often CVE data should be updated.
        /// The measurement is in days.
        /// </summary>
        public int CveTtl { get; set; } = 7;

        /// <summary>
        /// How often Image Scan data should be updated.
        /// The measurement is in hours.
        /// </summary>
        public int ImageScanTtl { get; set; } = 12;

        /// <summary>
        /// How often cached data should be updated by default.
        /// The measurement is in days.
        /// </summary>
        public int DefaultTtl { get; set; } = 7;
    }

    /// <summary>
    /// Azure Blob related configuration.
    /// </summary>
    public class AzureBlobConfig
    {
        /// <summary>
        /// Base Azure Blob Storage URL.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Sas token.
        /// </summary>
        public string Sas { get; set; }

        /// <summary>
        /// Azure Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Storage Account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Storage Account key.
        /// </summary>
        public string AccountKey { get; set; }
    }

    /// <summary>
    /// Watchmen related configurations.
    /// </summary>
    public class Watchmen
    {
        /// <summary>
        /// How often ScannerContainersWatchman is listing root-level containers.
        /// The measurement is in seconds.
        /// </summary>
        public int ScannerContainersPeriodicity { get; set; }

        /// <summary>
        /// How often ArchiveWatchman is maintaining the Archive.
        /// The measurement is in hours.
        /// </summary>
        public int ArchiverPeriodicityHours { get; set; } = 24;

        /// <summary>
        /// How long processed audits should be stored in Archive.
        /// The measurement is in days.
        /// </summary>
        public int ArchiveTtlDays { get; set; } = 90;
    }

    /// <summary>
    /// Azure Storage Queue related configuration.
    /// </summary>
    public class AzureQueueConfig
    {
        /// <summary>
        /// Azure Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Storage Account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Storage Account key.
        /// </summary>
        public string AccountKey { get; set; }

        /// <summary>
        /// The name of Image Scan Requests queue.
        /// </summary>
        public string ImageScanRequestsQueue { get; set; }
    }
}