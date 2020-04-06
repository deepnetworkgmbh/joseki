import { Component, Vue, Prop } from "vue-property-decorator";
import { InfrastructureComponent, ScoreHistoryItem, CountersSummary } from '@/models';
import router from '@/router';
import { ChartService } from '@/services/ChartService';
import { DateTime } from 'luxon';

@Component
export default class InfComponent extends Vue {

  @Prop()
  private component!: InfrastructureComponent;

  @Prop()
  private score: any;

  @Prop()
  private total: any;

  @Prop()
  private index: any;

  @Prop()
  private date?: DateTime;
  
  @Prop()
  private scoreHistory!: ScoreHistoryItem[];

  @Prop()
  private summary!: CountersSummary;

  get areaSeries() {
    return [{ data: this.scoreHistory.map((item)=> ({ x: item.recordedAt.split('T')[0] , y: item.score })).reverse() }]
  }

  get areaOptions() : ApexCharts.ApexOptions {
    return ChartService.AreaChartOptions(this.component.id, this.scoreHistory, [this.date!], [this.score], this.areaCallback);
  }

  get donutSeries() {
    return this.summary.getSeries();
  }

  get donutOptions(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut', this.summary);
  }

  areaCallback(date: string) {
    this.$emit('dateChanged', date);
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

  goComponentDetail(component: InfrastructureComponent) {
    let params = encodeURIComponent(component.id);
    if (this.date !== undefined) {
      params += '/' + this.date.toISODate();
    }
    router.push('/component-detail/' + params);
  }

}
