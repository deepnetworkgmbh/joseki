import { Component, Vue, Prop } from "vue-property-decorator";
import { CheckControl } from '@/services/MappingService';
import router from '@/router';
import { DiffOperation } from '@/services/MappingService';

@Component
export default class ControlList extends Vue {

  @Prop()
  control!: CheckControl;

  @Prop()
  date!: Date;

  getLineClass(): string {
    return `control-li text-sm control-${this.control.operation}`
  }
  goToImageScan(imageTag: string) {
    router.push('/image-detail/' + encodeURIComponent(imageTag) + '/' + encodeURIComponent(this.date.toDateString()));
  }
}