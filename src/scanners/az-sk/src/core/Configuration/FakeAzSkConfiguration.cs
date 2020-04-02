using System;

namespace core.Configuration
{
    /// <summary>
    /// Azure Security Kit fake scanner configuration.
    /// </summary>
    public class FakeAzSkConfiguration : IScannerConfiguration
    {
        /// <summary>
        /// Scanner identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// How often the scanner is scheduled to run.
        /// </summary>
        public string Periodicity { get; set; }

        /// <summary>
        /// Path to folder with pre-generated fake results.
        /// </summary>
        public string FakeResultsFolderPath { get; set; }
    }
}