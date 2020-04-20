import { InfrastructureComponent, CountersSummary, ScoreHistoryItem, Check } from '@/models';

export class InfrastructureComponentSummary {
  // date of the summary
  date: string = '';
  // the component of the summary
  component: InfrastructureComponent = new InfrastructureComponent()
  /// Latest known check-result counters.
  current!: CountersSummary
  /// Holds Scores per last 30 days.
  /// If no data for a day - places 0.
  scoreHistory: ScoreHistoryItem[] = []
  categorySummaries: any[] = []
  // true if this component summary does not
  // have a pair on overview diff
  notLoaded: boolean = false;
  checks: Check[] = [];
  

  public static fromData(data): InfrastructureComponentSummary {
    let result = <InfrastructureComponentSummary>data;
    if (result.component.category === 'Subscription') {
      result.component.category = 'Azure Subscription';
    }
    result.scoreHistory = result.scoreHistory.reverse();
    result.current = new CountersSummary(data.current);
    return result;
  }

}