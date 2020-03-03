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
            if (description.Contains("status=401"))
            {
                return NotAuthorized;
            }
            else if (description.Contains("failed to analyze OS: Unknown OS"))
            {
                return UnknownOS;
            }
            else
            {
                return UnknownError;
            }
        }
    }
}