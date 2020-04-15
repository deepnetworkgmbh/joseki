import { Component, Vue, Prop } from "vue-property-decorator";
import { SeverityFilter } from '@/models/SeverityFilter';

@Component
export default class ResultFilter extends Vue {

    @Prop()
    severities!: SeverityFilter;

    menuOpen: boolean = false;
}


