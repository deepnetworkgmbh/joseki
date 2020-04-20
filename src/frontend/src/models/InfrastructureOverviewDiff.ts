import { InfrastructureComponentSummary, InfrastructureOverview, InfrastructureComponent, ScoreHistoryItem } from '@/models';

export class InfrastructureOverviewDiff {
  summary1!: InfrastructureOverview
  summary2!: InfrastructureOverview
  compositeComponents: InfrastructureComponentSummary[] = [];

  public static fromData(data): InfrastructureOverviewDiff {
    let result = new InfrastructureOverviewDiff();
    result.summary1 = InfrastructureOverview.GenerateFromDiff(data.overall1, data.components1);
    result.summary2 = InfrastructureOverview.GenerateFromDiff(data.overall2, data.components2);
    result.compositeComponents = [];
    
    let cc:InfrastructureComponentSummary[] = []

    for(let i=0;i<result.summary1.components.length;i++) {
      let id = result.summary1.components[i].component.id;
      let index = cc.findIndex(x=>x.component.id === id);
      if(index === -1) {
        cc.push(result.summary1.components[i]);
      }
    }
    for(let i=0;i<result.summary2.components.length;i++) {
      let id = result.summary2.components[i].component.id;
      let index = cc.findIndex(x=>x.component.id === id);
      if(index === -1) {
        cc.push(result.summary2.components[i]);
      }
    }

    result.compositeComponents = cc;   
    console.log(`[]output`, JSON.parse(JSON.stringify(result)));      
    return result;
  }
}
