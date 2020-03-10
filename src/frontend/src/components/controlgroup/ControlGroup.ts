import { Component, Vue, Prop } from "vue-property-decorator";
import ControlList from "@/components/controllist/ControlList.vue";

@Component({
    components: { ControlList }
})
export default class ControlGroup extends Vue {

  @Prop()
  name!: any;

  @Prop()
  date!: Date;

  @Prop({ default: []})
  items!:any[]
}