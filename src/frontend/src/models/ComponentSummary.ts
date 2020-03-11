import { InfrastructureComponent, CountersSummary, ScoreHistoryItem, Check } from '@/models';

export class InfrastructureComponentSummary {
  // date of the summary
  date: string = '';
  // the component of the summary
  component: InfrastructureComponent = new InfrastructureComponent()
  /// Latest known check-result counters.
  current: CountersSummary = new CountersSummary()
  /// Holds Scores per last 30 days.
  /// If no data for a day - places 0.
  scoreHistory: ScoreHistoryItem[] = []
  sections: any[] = [];
  checks: Check[] = [];
  categorySummaries: any[] = []

  public static getSections(c: CountersSummary): any[] {
    let result: any[] = [];
    if (c.noData > 0) {
      result.push({ label: 'no data', value: c.noData, color: '#B7B8A8' });
    }
    if (c.failed > 0) {
      result.push({ label: 'error', value: c.failed, color: '#E33035' });
    }
    if (c.warning > 0) {
      result.push({ label: 'warning', value: c.warning, color: '#F8A462' });
    }
    if (c.passed > 0) {
      result.push({ label: 'success', value: c.passed, color: '#41C6B9' });
    }
    //console.log(`[] ${name}`, result);
    return result;
  }


}