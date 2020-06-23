import { ChartService } from '@/services/ChartService';

/// Represents short summary of check-result counters.
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

  constructor(data: any) {
    this.passed = data === undefined ? 0 : data.passed;
    this.failed = data === undefined ? 0 : data.failed;
    this.warning = data === undefined ? 0 : data.warning;
    this.noData = data === undefined ? 0 : data.noData;
    this.total = data === undefined ? 0 : data.total;
    this.score = this.calculateScore();
  }

  public calculateScore(): number {
    let result = Math.round(100 * this.passed * 2 / ((this.failed * 2) + (this.passed * 2) + this.warning));
    return isNaN(result) ? 0 : result;
  }

  public getString(): string {

    let result: string[] = [];
    if(this.noData>0) {
      result.push('No Data:' + this.noData);
    }
    if(this.failed>0) {
      result.push('Failed:' + this.failed);
    }
    if(this.warning>0) {
      result.push('Warning:' + this.warning);
    }
    if(this.passed>0) {
      result.push('Passed:' + this.passed);
    }
    return result.join(", ");

  }


  public getSeries(): number[] {
    let result: number[] = [];
    if(this.noData>0) { result.push(this.noData); }
    if(this.failed>0) { result.push(this.failed); }
    if(this.warning>0) { result.push(this.warning); }
    if(this.passed>0) { result.push(this.passed); }
    return result;
  }

  public getLabels(): string[] {
    let result: string[] = [];
    if(this.noData>0) { result.push('No Data'); }
    if(this.failed>0) { result.push('Failed'); }
    if(this.warning>0) { result.push('Warning'); }
    if(this.passed>0) { result.push('Success'); }
    return result;    
  }

  public getColors(): string[] {
    let result: string[] = [];
    if(this.noData>0) { result.push(ChartService.colorNoData); }
    if(this.failed>0) { result.push(ChartService.colorFailed); }
    if(this.warning>0) { result.push(ChartService.colorWarning); }
    if(this.passed>0) { result.push(ChartService.colorSuccess); }
    return result;    
  }

}