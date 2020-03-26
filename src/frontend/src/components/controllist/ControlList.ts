import { Component, Vue, Prop } from "vue-property-decorator";
import { CheckControl } from '@/services/DiffService';
import router from '@/router';
import { DateTime } from 'luxon';

@Component
export default class ControlList extends Vue {

  @Prop()
  control!: CheckControl;

  @Prop()
  date!: DateTime;

  @Prop({default: false})
  smallWidth!: boolean

  getIdClass(): string {
    return this.smallWidth ? `limited-li` : ``;
  }

  getLineClass(): string {
    return `control-li text-sm control-${this.control.operation}`
  }

  get imageScanUrl() { 
    return '/image-detail/' + encodeURIComponent(this.control.tags['imageTag']) + '/' + this.date.toISODate();
  }
}