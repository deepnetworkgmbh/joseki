import { InfrastructureComponentSummary, InfrastructureOverview, InfrastructureComponent, ScoreHistoryItem } from '@/models';

export class InfrastructureOverviewDiff {
  summary1!: InfrastructureOverview
  summary2!: InfrastructureOverview
  compositeComponents: InfrastructureComponentSummary[] = [];
}
