
/// <summary>
/// Represents short summary of check-result counters.
/// </summary>
export class CountersSummary {
  /// Amount of Passed checks.
  public passed: number = 0

  /// Amount of failed checks.
  public failed: number = 0

  /// Amount of Warnings.
  public warning: number = 0

  /// Amount of checks with no-data result: requires a manual verification or Joseki is not able to perform the check.
  public noData: number = 0

  /// Total checks amount.
  public total: number = 0;

  /// The audit score. It indicates how close the infrastructure is to known best-practices configuration.
  /// NoData checks are excluded, Passed and Failed has doubled weight.
  public score: number = 0;

  public calculateScore() {
    var result = Math.round(100 * this.passed * 2 / ((this.failed * 2) + (this.passed * 2) + this.warning));
    return isNaN(result) ? 0 : result;
  }

}