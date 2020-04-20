import { Component, Vue, Prop } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { ChartService } from '@/services/';
import { InfrastructureComponent, ScoreHistoryItem, CountersSummary } from '@/models';

@Component
export default class InfComponent extends Vue {

  @Prop()
  private component!: InfrastructureComponent;

  @Prop()
  private score: any;

  @Prop()
  private index: any;

  @Prop()
  private date?: DateTime;
  
  @Prop()
  private scoreHistory!: ScoreHistoryItem[];

  @Prop()
  private summary!: CountersSummary;

  /**
   * Series getter for area chart.
   *
   * @readonly
   * @memberof InfComponent
   */
  get areaSeries() {
    return [{ data: this.scoreHistory.map((item)=> ({ x: item.recordedAt.split('T')[0] , y: item.score })).reverse() }]
  }

  /**
   * Options getter for area chart.
   *
   * @readonly
   * @type {ApexCharts.ApexOptions}
   * @memberof InfComponent
   */
  get areaOptions() : ApexCharts.ApexOptions {
    return ChartService.AreaChartOptions(this.component.id, this.scoreHistory, [this.date!], [this.score], this.areaCallback);
  }

  /**
   * Series getter for donut chart.
   *
   * @readonly
   * @memberof InfComponent
   */
  get donutSeries() {
    return this.summary.getSeries();
  }

  /**
   * Options getter for donut chart.
   *
   * @readonly
   * @type {ApexCharts.ApexOptions}
   * @memberof InfComponent
   */
  get donutOptions(): ApexCharts.ApexOptions {
    return ChartService.DonutChartOptions(this.component.id + '_donut', this.summary, this.donutCallback);
  }

  /**
   * Callback function for when the area chart is clicked.
   *
   * @param {string} date
   * @memberof InfComponent
   */
  areaCallback(date: string) {
    this.$emit('dateChanged', date);
  }

  /**
   * Callback function for when the donut chart is clicked.
   *
   * @param {string} status
   * @memberof InfComponent
   */
  donutCallback(status: string) {
    let filterBy = btoa(`result=${status}&component=${this.component.name}`);
    let dateStr = this.date!.toISODate();
    router.push(`/overview-detail/${dateStr}/${filterBy}`); 
  }
  
   /**
   * Returns component category icon.
   *
   * @param {string} category
   * @returns
   * @memberof InfComponent
   */
  getComponentIcon() { return InfrastructureComponent.getIcon(this.component.category) }

  /**
   * Navigates to component history page.
   *
   * @memberof InfComponent
   */
  goComponentHistory() {
    router.push('/component-history/' + encodeURIComponent(this.component.id));
  }

  /**
   * Navigates to component detail page.
   *
   * @memberof InfComponent
   */
  goComponentDetail() {
    let params = encodeURIComponent(this.component.id);
    if (this.date !== undefined) {
      params += '/' + this.date.toISODate();
    }
    router.push('/component-detail/' + params);
  }

}
