export class ScoreService {

  public static getOrderBySeverity(severity: string): number {
    switch (severity) {
      case 'CRITICAL': return 10;
      case 'HIGH': return 9;
      case 'MEDIUM': return 8;
      case 'LOW': return 7;
      case 'UNKNOWN': return 0;
    }
    return 0;
  }

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

  public static getScoreIconClass(score: number): string {
    let result = '';

    if (score > 0 && score <= 25) {
      result = "fa fa-poo-storm";
    }
    if (score > 25 && score <= 50) {
      result = "fa fa-cloud-rain"
    }
    if (score > 50 && score <= 75) {
      result = "fa fa-cloud-sun"
    }
    if (score > 75) {
      result = "fa fa-sun";
    }
    return result;
  }
}