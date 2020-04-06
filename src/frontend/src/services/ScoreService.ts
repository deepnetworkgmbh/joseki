import { CheckSeverity } from '@/models/CheckSeverity'

/**
 * Score service contains methods related with score calculations, ordering or view icon
 *
 * @export
 * @class ScoreService
 */
export class ScoreService {
  
  /**
   * return class for CheckSeverity
   * 
   * @static
   * @param {CheckSeverity} severity
   * @returns {string}
   * @memberof ScoreService
   */
  public static getControlIcon(severity: CheckSeverity): string {
    switch (severity.toString()) {
      case 'NoData':
        return 'icon-x-circle nodata-icon'
      case 'Failed':
        return 'icon-x-circle failed-icon'
      case 'Warning':
        return 'icon-alert-triangle warning-icon'
      case 'Success':
        return 'icon-check-circle noissues-icon'
    }
    return ''
  }

  /**
   * return severity score, used for ordering
   *
   * @static
   * @param {CheckSeverity} severity
   * @returns {number}
   * @memberof ScoreService
   */
  public static getSeverityScore(severity: CheckSeverity): number {
    switch (severity.toString()) {
      case 'Success':
        return 1
      case 'NoData':
        return 10
      case 'Warning':
        return 50
      case 'Failed':
        return 100
    }
    return 0;
  }

  /**
   * returns severity order
   *
   * @static
   * @param {string} severity
   * @returns {number}
   * @memberof ScoreService
   */
  public static getOrderBySeverity(severity: string): number {
    switch (severity) {
      case 'Critical': return 10;
      case 'High': return 9;
      case 'Medium': return 8;
      case 'Low': return 7;
      case 'Unknown': return 0;
    }
    return 0;
  }

  /**
   * calculates grade using score
   *
   * @static
   * @param {number} score
   * @returns {string}
   * @memberof ScoreService
   */
  public static getGrade(score: number): string {

    if (score > 97) {
      return 'A+'
    } else if (score >= 93 && score <= 96) {
      return 'A'
    } else if (score >= 90 && score <= 92) {
      return 'A-'
    } else if (score >= 87 && score <= 89) {
      return 'B+'
    } else if (score >= 83 && score <= 86) {
      return 'B'
    } else if (score >= 80 && score <= 82) {
      return 'B-'
    } else if (score >= 77 && score <= 79) {
      return 'C+'
    } else if (score >= 73 && score <= 76) {
      return 'C'
    } else if (score >= 70 && score <= 72) {
      return 'C-'
    } else if (score >= 67 && score <= 69) {
      return 'D+'
    } else if (score >= 63 && score <= 66) {
      return 'D'
    } else if (score >= 60 && score <= 62) {
      return 'D-'
    } else {
      return 'F'
    }
  }

}