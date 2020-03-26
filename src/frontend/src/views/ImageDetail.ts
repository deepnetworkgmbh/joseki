import { Component, Vue, Prop } from "vue-property-decorator";
import { DataService } from "@/services/DataService";
import Spinner from "@/components/spinner/Spinner.vue";
import { ImageScanDetailModel, InfrastructureComponent } from '@/models';

@Component({
  components: { Spinner }
})
export default class ImageDetail extends Vue {
  @Prop()
  imageid!: string;

  @Prop({ default: null })
  date!: string;

  @Prop()
  component?: InfrastructureComponent

  loaded: boolean = false;
  service: DataService = new DataService();
  data: ImageScanDetailModel = new ImageScanDetailModel();

  created() {
    this.service.getImageScanResultData(this.imageid, this.date)
      .then(response => {
        if (response) {
          this.data = response;
          this.loaded = true;
          this.setupPage();
        }
      });

    if(this.component) {
      console.log(`[] component=>`, this.component.id);
      this.$emit('componentChanged', this.component);
    }
  }

  setupPage() { }
}
