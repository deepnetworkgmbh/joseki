import { Component, Vue, Prop } from "vue-property-decorator";
import { CountersSummary } from '@/models';
import { MetaService } from '@/services/MetaService';

@Component
export default class StatusBar extends Vue {
  @Prop()
  counters!: CountersSummary;

  @Prop({ default: false })
  mini!: boolean

  private useNoData = true;
  
  private starts: boolean[] = [false, false, false, false];
  private ends: boolean[] = [false, false, false, false];
  private classes: string[] = ["nodata", "failed", "warning", "passed"]
  private widths: number[] = [0,0,0,0]

  // {{ counters.noData }}, {{ counters.failed }}, {{ counters.warning }}, {{ counters.passed }}
  created() {

    let arr = [ (this.useNoData ? this.counters.noData : 0) , this.counters.failed, this.counters.warning, this.counters.passed];

    // calculate start-ends
    for(let i=0;i<arr.length;i++) {
      if(arr[i]>0) { this.starts[i] = true; break;}
    }
    for(let i=0;i<arr.length;i++) {
      if(arr[i] > 0) { for(let j=0;j<i;j++){ this.ends[j]=false;} this.ends[i] = true;}
    }

    let sum = arr.reduce((a,b)=> a+b);
    // calculate widths
    for(let i=0;i<arr.length;i++) {
        if(arr[i]>0) {
          let width = Math.ceil(200 * arr[i] / sum);
          if(width>2) {
            width-=1;
          }
          this.widths[i] = width;
        }
    }

    let widthSum = this.widths.reduce((a, b)=>a+b);

    if(widthSum<200) {
      // adjust widths
      let min = 200;    
      let minIndex = -1;
      for(let i=0;i<this.widths.length;i++) {
        if(this.widths[i]>0) {
          if (min> this.widths[i]) { 
            min = this.widths[i];
            minIndex = i;
          }          
        } 
      }
      if (minIndex !== -1) {
        this.widths[minIndex] += (200 - widthSum);
      }
    }

  }

  getWidth(i: number) {
    return this.widths[i] + 'px';
  }

  getClass(i: number): string {
    let result = "statusbar-" + this.classes[i];
    if(this.starts[i]) {
      result += " statusbar-" + this.classes[i] + "-start"
    }
    if(this.ends[i]) {
      result += " statusbar-" + this.classes[i] + "-end"
    }
    return result;
  }
  meta(key: string) { return MetaService.Get(key) }

}
