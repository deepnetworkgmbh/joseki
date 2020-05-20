import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { CountersSummary } from '@/models';
import { MetaService } from '@/services/MetaService';
import { SeverityFilter } from '@/models/SeverityFilter';

@Component
export default class StatusBar extends Vue {
  @Prop()
  counters!: CountersSummary;

  @Prop({ default: false })
  mini!: boolean

  @Prop({ default: ()=> new SeverityFilter() })
  severities!: SeverityFilter;

  private starts: boolean[] = [false, false, false, false];
  private ends: boolean[] = [false, false, false, false];
  private classes: string[] = ["nodata", "failed", "warning", "passed"]
  public widths: number[] = [0,0,0,0];
  public array: number[] = [0,0,0,0];

  renderPortions() {
    this.starts = [false, false, false, false];
    this.ends = [false, false, false, false];
    this.widths = [0,0,0,0];

    this.array = [ 
      (this.severities.nodata ? this.counters.noData : 0) , 
      (this.severities.failed ? this.counters.failed : 0), 
      (this.severities.warning ? this.counters.warning : 0),
      (this.severities.success ? this.counters.passed : 0)
    ];

    // calculate start-ends
    for(let i=0;i<this.array.length;i++) {
      if(this.array[i]>0) { this.starts[i] = true; break;}
    }
    for(let i=0;i<this.array.length;i++) {
      if(this.array[i] > 0) { for(let j=0;j<i;j++){ this.ends[j]=false;} this.ends[i] = true;}
    }

    let sum = this.array.reduce((a,b)=> a+b);
    // calculate widths
    for(let i=0;i<this.array.length;i++) {
        if(this.array[i]>0) {
          let width = Math.ceil(200 * this.array[i] / sum);
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

  getLabel(i:number): string {
    return '';
    // return this.widths[i]> 10 ? this.array[i].toString() : '';
  }

  meta(key: string) { return MetaService.Get(key) }

  @Watch('severities', {immediate:true, deep: true})
  onSeverityFilterChanged(newValue: SeverityFilter) {
    this.renderPortions();
  }
}
