using System;
using System.Runtime.Serialization;

namespace webapp.Models
{
    /// <summary>
    /// Scan result enum.
    /// </summary>
    public enum ScanResult
    {
        /// <summary>
        /// Enum value when a scan was not found
        /// </summary>
        [EnumMember(Value = "NOT_FOUND")]
        NotFound = 0,

        /// <summary>
        /// Enum value when a scan faile d
        /// </summary>
        [EnumMember(Value = "FAILED")]
        Failed = 1,

        /// <summary>
        /// Enum value when a scan succeeded
        /// </summary>
        [EnumMember(Value = "SUCCEEDED")]
        Succeeded = 2,
    }
}
