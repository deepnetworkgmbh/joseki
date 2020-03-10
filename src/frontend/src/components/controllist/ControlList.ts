import { Component, Vue, Prop } from "vue-property-decorator";
import router from '@/router';

@Component
export default class ControlList extends Vue {

  @Prop()
  control!: any;

  @Prop()
  date!: Date;

  goToImageScan(imageTag: string) {
    router.push('/image-detail/' + encodeURIComponent(imageTag) + '/' + encodeURIComponent(this.date.toDateString()));
  }
}