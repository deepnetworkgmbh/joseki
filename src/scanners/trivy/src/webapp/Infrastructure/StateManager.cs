using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace webapp.Infrastructure
{
    /// <summary>
    /// Keeps the state of Image Scanner service.
    /// </summary>
    public static class StateManager
    {
        private static bool isReady;

        private static bool isLive;

        /// <summary>
        /// switches the service into ready-to-receive-traffic state.
        /// </summary>
        public static void SetReady() => isReady = true;

        /// <summary>
        /// Switches the service into Live state.
        /// </summary>
        public static void SetLive() => isLive = true;

        /// <summary>
        /// Indicates if service is ready to receive external traffic.
        /// </summary>
        public static HealthCheckResult Ready => isReady ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();

        /// <summary>
        /// Indicates if service requires a restart.
        /// </summary>
        public static HealthCheckResult Live => isLive ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}