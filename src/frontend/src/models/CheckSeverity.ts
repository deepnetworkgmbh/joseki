/// <summary>
/// Severity of the check.
/// </summary>
export enum CheckSeverity {
  /// <summary>
  /// Enum value when a scan was not found
  /// </summary>
  NoData = "NO_DATA",

  /// <summary>
  /// Enum value when a scan failed
  /// </summary>
  Failed = "FAILED",

  /// <summary>
  /// Enum value when a scan has warning
  /// </summary>
  Warning = "WARNING",

  /// <summary>
  /// Enum value when a scan succeeded
  /// </summary>
  Success = "SUCCESS",
}