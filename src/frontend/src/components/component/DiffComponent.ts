import { Component, Vue, Prop } from "vue-property-decorator";
import { InfrastructureComponent } from '@/models';
import router from '@/router';
import { ChartService } from '@/services/ChartService';
import { DateTime } from 'luxon';

@Component
export default class DiffComponent extends Vue {

  @Prop()
  private component: any;

  @Prop()
  private sections: any;

  @Prop()
  private score: any;

  @Prop()
  private total: any;

  @Prop()
  private index: any;

  @Prop()
  private date!: string;

  @Prop()
  private sections2: any;

  @Prop()
  private score2: any;

  @Prop()
  private total2: any;

  @Prop()
  private date2!: string;

  @Prop()
  private notLoaded!: boolean;

  getComponentIcon() {
    if(this.component.category === 'Azure Subscription') {
      return 'icon-azuredevops';
    }
    if(this.component.category === 'Kubernetes') {
      return 'icon-kubernetes';
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
