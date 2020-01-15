import { Component, Vue, Prop } from "vue-property-decorator";
import { DataService } from "@/services/DataService";
import Spinner from "@/components/spinner/Spinner.vue";
import { ContainerImageScan } from "@/models";

@Component({
  components: { Spinner }
})
export default class ImageDetail extends Vue {
  @Prop({ default: "" })
  imageid!: string;
  loaded: boolean = false;
  service: DataService = new DataService();
  data: ContainerImageScan = new ContainerImageScan();

  created() {
    this.imageid = decodeURIComponent(this.$route.params.imageid);
    console.log(`getting data for image ${this.imageid}`);
    this.service.getImageScanResultData(this.imageid).then(response => {
      console.log(response);
      this.data = response;
      this.loaded = true;
      console.log(this.data);
      this.setupPage();
    });
  }

  setupPage() {}
}
