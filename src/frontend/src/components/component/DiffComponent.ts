import { Component, Vue, Prop } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { InfrastructureComponent, ScoreHistoryItem, CountersSummary } from '@/models';
import { ChartService } from '@/services/';

@Component
export default class DiffComponent extends Vue {

  @Prop()
  private component!: InfrastructureComponent;

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

  /**
   * Series getter for area chart.
   *
   * @readonly
   * @memberof DiffComponent
   */
  get areaSeries() {
    return ScoreHistoryItem.getInterpolatedThresholdSeries(this.scoreHistory);
  }

  /**
   * Options getter for area chart.
   *
   * @readonly
   * @type {ApexCharts.ApexOptions}
   * @memberof DiffComponent
   */
  get areaOptions() : ApexCharts.ApexOptions {
    const dates = [DateTime.fromISO(this.date), DateTime.fromISO(this.date2)]
    const scores= [
      this.summary1 === undefined ? 0 : this.summary1.score, 
      this.summary2 === undefined ? 0 : this.summary2.score    
    ]
    return ChartService.AreaChartOptions(this.component.id, this.scoreHistory, dates, scores, ()=>{});
  }

  /**
   * Series getter for 1st scan donut chart.
   *
   * @readonly
   * @memberof DiffComponent
   */
  get donutSeries1() {
    if(this.summary1 === undefined) return [1];
    return this.summary1.getSeries();
  }

  /**
   * Options getter for 1st scan donut chart.
   *
   * @readonly
   * @type {ApexCharts.ApexOptions}
   * @memberof DiffComponent
   */
  get donutOptions1(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut1', this.summary1, this.donutCallback1);
  }

  /**
   * Series getter for 2nd scan donut chart.
   *
   * @readonly
   * @memberof DiffComponent
   */
  get donutSeries2() {
    if(this.summary2 === undefined) return [1];
    return this.summary2.getSeries();
  }

  /**
   * Options getter for 2nd scan donut chart.
   *
   * @readonly
   * @type {ApexCharts.ApexOptions}
   * @memberof DiffComponent
   */
  get donutOptions2(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut2', this.summary2, this.donutCallback2);
  }

  /**
   * Callback function for when the 1st donut is clicked.
   *
   * @param {string} status
   * @memberof DiffComponent
   */
  donutCallback1(status: string) {
    let filterBy = btoa(`result=${status}&component=${this.component.name}`);
    router.push(`/overview-detail/${this.date}/${filterBy}`); 
  }

  /**
   * Callback function for when the 2nd donut is clicked.
   *
   * @param {string} status
   * @memberof DiffComponent
   */
  donutCallback2(status: string) {
    let filterBy = btoa(`result=${status}&component=${this.component.name}`);
    router.push(`/overview-detail/${this.date2}/${filterBy}`); 
  }

  /**
   * Returns component category icon.
   *
   * @param {string} category
   * @returns
   * @memberof DiffComponent
   */
  getComponentIcon() { return InfrastructureComponent.getIcon(this.component.category) }
  
  /**
   * Navigates to component history page.
   *
   * @param {InfrastructureComponent} component
   * @memberof DiffComponent
   */
  goComponentHistory() {
    router.push('/component-history/' + this.component.id);
  }

  /**
   * Navigates to component diff page.
   *
   * @memberof DiffComponent
   */
  goComponentDiff() {
    let params = encodeURIComponent(this.component.id);
    params += '/' + this.date + '/' + this.date2;
    router.push('/component-diff/' + params);
  }

}
