import { Component, Vue, Prop } from "vue-property-decorator";
import { NamespaceCounters } from "@/models";

@Component
export default class StatusBar extends Vue {
  @Prop()
  counters!: NamespaceCounters;

  private sum: number = 0;
  private failingSum: number = 0;
  private noDataSum: number = 0;

  created(){
    this.sum = this.counters.passing + this.counters.failing + this.counters.nodata;
    this.failingSum = Math.round((this.counters.failing * 200) / this.sum);
    this.noDataSum = Math.round((this.counters.nodata * 200) / this.sum);
  }

  noDataWidth() {
    const noDataPx = 200 - this.failingSum;
    return noDataPx + 'px';    
  }

  passingWidth() {
    const passingPx = 200 - this.failingSum - this.noDataSum;
    return passingPx + 'px';
  }
}
