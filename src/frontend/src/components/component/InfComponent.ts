import { Component, Vue, Prop } from "vue-property-decorator";
import { InfrastructureComponent } from '@/models';
import router from '@/router';
import { ChartService } from '@/services/ChartService';
import { DateTime } from 'luxon';

@Component
export default class InfComponent extends Vue {

  @Prop()
  private component!: InfrastructureComponent;

  @Prop()
  private sections: any;

  @Prop()
  private score: any;

  @Prop()
  private total: any;

  @Prop()
  private index: any;

  @Prop()
  private date?: DateTime;
  
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

  goComponentDetail(component: InfrastructureComponent) {
    let params = encodeURIComponent(component.id);
    if (this.date !== undefined) {
      params += '/' + this.date.toISODate();
    }
    router.push('/component-detail/' + params);
  }

}
