import { InfrastructureComponentSummary } from '@/models';

export class InfrastructureOverviewDiff {
  /// First overall infrastructure summary.
  overall1: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Second overall infrastructure summary.
  overall2: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  ///Components of first summary.
  components1: InfrastructureComponentSummary[] = [];
  ///Components of second summary.
  components2: InfrastructureComponentSummary[] = [];
}
