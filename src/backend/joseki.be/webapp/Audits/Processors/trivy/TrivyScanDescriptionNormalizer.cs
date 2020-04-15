using System;

namespace webapp.Audits.Processors.trivy
{
    /// <summary>
    /// The object normalizes trivy output.
    /// </summary>
    public static class TrivyScanDescriptionNormalizer
    {
        /// <summary>
        /// Human-friendly not-authorized message.
        /// </summary>
        public const string NotAuthorized = "Trivy is not authorized to pull the image";

        /// <summary>
        /// Human-friendly Unknown-OS message.
        /// </summary>
        public const string UnknownOS = "Trivy is not able to scan underlying OS";

        /// <summary>
        /// Human-friendly FailedToAnalyzePackages message.
        /// </summary>
        public const string FailedToGetPackages = "Trivy is not able to get package info from the package manager. Maybe base image does not have package manager?";

        /// <summary>
        /// Human-friendly NoPackages message.
        /// </summary>
        public const string NoPackages = "Trivy did not found any package in the given docker image. Is package manager properly installed there?";

        /// <summary>
        /// Human-friendly Unknown-Error message.
        /// </summary>
        public const string UnknownError = "Unknown error occured";

        /// <summary>
        /// Converts trivy output to human-friendly text.
        /// </summary>
        /// <param name="description">Trivy scan output.</param>
        /// <returns>Human readable result description.</returns>
        public static string ToHumanReadable(string description)
        {
            if (description.Contains("status=401", StringComparison.InvariantCultureIgnoreCase))
            {
                return NotAuthorized;
            }
            else if (description.Contains("unknown OS", StringComparison.InvariantCultureIgnoreCase))
            {
                return UnknownOS;
            }
            else if (description.Contains("failed to get packages", StringComparison.InvariantCultureIgnoreCase))
            {
                return FailedToGetPackages;
            }
            else if (description.Contains("no packages detected", StringComparison.InvariantCultureIgnoreCase))
            {
                return NoPackages;
            }
            else
            {
                return UnknownError;
            }
        }
    }
}