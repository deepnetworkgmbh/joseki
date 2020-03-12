import { Component, Vue, Prop } from "vue-property-decorator";
import ControlList from "@/components/controllist/ControlList.vue";
import { CheckControl } from '@/services/MappingService';

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
}