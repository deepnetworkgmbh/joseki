import { Component, Vue, Prop } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { InfrastructureComponent, ScoreHistoryItem, CountersSummary } from '@/models';
import { ChartService } from '@/services/';

@Component
export default class DiffComponent extends Vue {

  @Prop()
  private component: any;

  @Prop()
  private index: any;

  @Prop()
  private date!: string;

  @Prop()
  private date2!: string;

  @Prop()
  private notLoaded!: boolean;

  @Prop()
  private scoreHistory!: ScoreHistoryItem[];

  @Prop()
  private summary1!: CountersSummary;

  @Prop()
  private summary2!: CountersSummary;

  get areaSeries() {
    return [{ data: this.scoreHistory.map((item)=> ({ x: item.recordedAt.split('T')[0] , y: item.score })).reverse() }]
  }

  get areaOptions() : ApexCharts.ApexOptions {
    const dates = [DateTime.fromISO(this.date), DateTime.fromISO(this.date2)]
    const scores= [
      this.summary1 === undefined ? 0 : this.summary1.score, 
      this.summary2 === undefined ? 0 : this.summary2.score    
    ]
    return ChartService.AreaChartOptions(this.component.id, this.scoreHistory, dates, scores, this.areaCallback);
  }

  get donutSeries1() {
    if(this.summary1 === undefined) return [1];
    return this.summary1.getSeries();
  }

  get donutOptions1(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut1', this.summary1, this.donutCallback1);
  }

  get donutSeries2() {
    if(this.summary2 === undefined) return [1];
    return this.summary2.getSeries();
  }

  get donutOptions2(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut2', this.summary2, this.donutCallback1);
  }

  donutCallback1(status: string) {
    let filterBy = btoa(`result=${status}&component=${this.component.name}`);
    router.push({ name: 'OverviewDetail', params: { date: this.date, filter: filterBy, sort: '' } });
  }

  donutCallback2(status: string) {
    let filterBy = btoa(`result=${status}&component=${this.component.name}`);
    router.push({ name: 'OverviewDetail', params: { date: this.date2, filter: filterBy, sort: '' } });
  }

  areaCallback(){

  }

  getComponentIcon() {
    if(this.component.category === 'Azure Subscription') {
      return 'component-icon-azure icon-azuredevops';
    }
    if(this.component.category === 'Kubernetes') {
      return 'component-icon-kubernetes icon-kubernetes';
    }
    return ''
  }
  
  goComponentHistory(component: InfrastructureComponent) {
    if (component) {
      router.push('/component-history/' + component.id);
    } else {
      router.push('/component-history/');
    }
  }

  goComponentDiff(component: InfrastructureComponent) {
    let params = encodeURIComponent(component.id);
    params += '/' + this.date + '/' + this.date2;
    router.push('/component-diff/' + params);
  }

}
