import { InfrastructureComponentSummary, InfrastructureComponent } from '@/models';
import { CountersSummary } from './CounterSummary';

export class InfrastructureOverview {
    /// Overall infrastructure summary.
    overall: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    /// Separate summary for each involved component.
    components: InfrastructureComponentSummary[] = [];

    public static GenerateFromDiff(overall: InfrastructureComponentSummary, components: InfrastructureComponentSummary[])
      : InfrastructureOverview
    {
      let data = { overall: overall, components: components }      
      return InfrastructureOverview.GenerateFromData(data);
    }

    public static GenerateFromData(data: any): InfrastructureOverview {
      let result = new InfrastructureOverview();
      result.overall = <InfrastructureComponentSummary>data.overall;
      result.overall.current = new CountersSummary(data.overall.current);

      // reverse and slice overall history
      if(result.overall.scoreHistory) {
        result.overall.scoreHistory = result.overall.scoreHistory.reverse();//.slice(0, 14);
      }
      result.components = data.components;

      // generate sections for components
      for (let i = 0; i < result.components.length; i++) {
        if (result.components[i].component.category === 'Subscription') {
          result.components[i].component.category = 'Azure Subscription';
        }
        result.components[i].scoreHistory = result.components[i].scoreHistory.reverse(); //.slice(0, 14);
        result.components[i].current = new CountersSummary(result.components[i].current);
      }

      return result;
    }
}