using System;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace webapp.Infrastructure
{
    /// <summary>
    /// Knows when Joseki is Ready and Healthy.
    /// </summary>
    public static class JosekiStateManager
    {
        private static Watchman initializedWatchmen = Watchman.None;

        /// <summary>
        /// Marks Archiver as initialized.
        /// </summary>
        public static void ArchiverIsInitialized()
        {
            initializedWatchmen |= Watchman.Archiver;
        }

        /// <summary>
        /// Marks Scanner Containers watchman as initialized.
        /// </summary>
        public static void ScannerContainersIsInitialized()
        {
            initializedWatchmen |= Watchman.ScannerContainers;
        }

        /// <summary>
        /// Marks Infrastructure Score cache as initialized.
        /// </summary>
        public static void ScoreCacheIsInitialized()
        {
            initializedWatchmen |= Watchman.ScoreCache;
        }

        /// <summary>
        /// Joseki is ready to receive traffic, when infra-score cache is pre-loaded.
        /// </summary>
        public static HealthCheckResult Ready =>
            (initializedWatchmen & Watchman.ScoreCache) == Watchman.ScoreCache
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();

        /// <summary>
        /// Joseki is not in "live" state, if any watchman is not properly started.
        /// </summary>
        public static HealthCheckResult Live =>
            initializedWatchmen == Watchman.All
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
    }

    /// <summary>
    /// Indicates what watchmen were started.
    /// </summary>
    [Flags]
    public enum Watchman
    {
        /// <summary>
        /// No pre-processors has been finished.
        /// </summary>
        None = 0,

        /// <summary>
        /// Infrastructure score cache is warmed up.
        /// </summary>
        ScoreCache = 1,

        /// <summary>
        /// Archiver is started.
        /// </summary>
        Archiver = 2,

        /// <summary>
        /// Scanner Containers watchman is started.
        /// </summary>
        ScannerContainers = 4,

        /// <summary>
        /// All are initialized.
        /// </summary>
        All = 7,
    }
}