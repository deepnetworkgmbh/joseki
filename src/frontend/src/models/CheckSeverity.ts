/// Severity of the check.
export enum CheckSeverity {
  /// Enum value when a scan was not found
  NoData = "NO_DATA",
  /// Enum value when a scan failed
  Failed = "FAILED",
  /// Enum value when a scan has warning
  Warning = "WARNING",
  /// Enum value when a scan succeeded
  Success = "SUCCESS",
}