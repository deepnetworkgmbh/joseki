import { Component, Vue, Prop } from "vue-property-decorator";
import { CountersSummary } from '@/models/InfrastructureOverview';

@Component
export default class StatusBar extends Vue {
  @Prop()
  counters!: CountersSummary;

  @Prop({ default: false })
  mini!: boolean

  private sum: number = 0;
  private failingSum: number = 0;
  private noDataSum: number = 0;

  created() {
    this.sum = this.counters.passed + this.counters.failed + this.counters.noData;
    this.failingSum = Math.round((this.counters.failed * 200) / this.sum);
    this.noDataSum = Math.round((this.counters.noData * 200) / this.sum);
  }

  noDataWidth() {
    const noDataPx = 200 - this.failingSum;
    return noDataPx + 'px';
  }

  passingWidth() {
    const passingPx = 200 - this.failingSum - this.noDataSum;
    return passingPx + 'px';
  }

  get height() {
    return this.mini ? '8px' : '12px';
  }
}
