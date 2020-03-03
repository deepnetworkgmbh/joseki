import { Component, Vue, Prop } from "vue-property-decorator";
import { InfrastructureComponent } from '@/models/InfrastructureOverview';
import router from '@/router';
import { ChartService } from '@/services/ChartService';

@Component
export default class InfComponent extends Vue {

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

  private showButtons: boolean = true;



  goComponentHistory(component: InfrastructureComponent) {
    if (component) {
      router.push('/component-history/' + component.id);
    } else {
      router.push('/component-history/');
    }
  }

  goComponentDetail(component: InfrastructureComponent, date: string) {
    console.log(`going component detail: ${component.id}`);
    let params = component.id;
    if (date) {
      params += '/' + date;
    }
    router.push('/component-detail/' + params);
  }

}
