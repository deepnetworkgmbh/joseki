import { Component, Vue, Prop } from "vue-property-decorator";
import { InfrastructureComponent } from '@/models/InfrastructureOverview';
import router from '@/router';
import { ChartService } from '@/services/ChartService';

@Component
export default class Score extends Vue {

  @Prop({ default: 0 })
  private score: any;

  @Prop({ default: '' })
  private label: any;

  get scoreClass(): string {
    if (this.score > 0 && this.score < 25) return 'scan-score scan-score-0';
    else if (this.score >= 25 && this.score < 50) return 'scan-score scan-score-25'
    else if (this.score >= 50 && this.score < 75) return 'scan-score scan-score-50';
    else if (this.score >= 75) return 'scan-score scan-score-75';
    return 'scan-score';
  }
}
