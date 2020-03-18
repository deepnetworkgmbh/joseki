import { Component, Vue, Prop } from "vue-property-decorator";
import { CheckControl } from '@/services/DiffService';
import router from '@/router';

@Component
export default class ControlList extends Vue {

  @Prop()
  control!: CheckControl;

  @Prop()
  date!: Date;

  getLineClass(): string {
    return `control-li text-sm control-${this.control.operation}`
  }

  get imageScanUrl() { 
    let dateParam = new Date(this.date).toDateString();
    return '/image-detail/' + encodeURIComponent(this.control.tags['imageTag']) + '/' + encodeURIComponent(dateParam) 
  }
}