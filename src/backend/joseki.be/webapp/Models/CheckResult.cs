using System;
using System.Runtime.Serialization;

namespace webapp.Models
{
    /// <summary>
    /// Severity of the check.
    /// </summary>
    public enum CheckResult
    {
        /// <summary>
        /// Enum value when a scan was not found
        /// </summary>
        [EnumMember(Value = "NO_DATA")]
        NoData = 0,

        /// <summary>
        /// Enum value when a scan failed
        /// </summary>
        [EnumMember(Value = "FAILED")]
        Failed = 1,

        /// <summary>
        /// Enum value when a scan has warning
        /// </summary>
        [EnumMember(Value = "WARNING")]
        Warning = 2,

        /// <summary>
        /// Enum value when a scan succeeded
        /// </summary>
        [EnumMember(Value = "SUCCESS")]
        Success = 255,
    }
}
