import { Component, Vue, Prop } from "vue-property-decorator";
import ControlList from "@/components/controllist/ControlList.vue";
import { CheckControl, DiffOperation } from '@/services/DiffService';

@Component({
    components: { ControlList }
})
export default class ControlGroup extends Vue {

  @Prop()
  name!: any;

  @Prop()
  date!: Date;

  @Prop({ default: []})
  items!:CheckControl[]

  @Prop()
  operation!: DiffOperation

}