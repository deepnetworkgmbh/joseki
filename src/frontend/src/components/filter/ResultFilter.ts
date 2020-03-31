import { Component, Vue, Prop } from "vue-property-decorator";
import { SeverityFilter } from '@/models/SeverityFilter';

@Component
export default class ResultFilter extends Vue {

    @Prop()
    severities!: SeverityFilter;

    menuOpen: boolean = false;

    created() {
        console.log(`[] severities`, this.severities);
    }

    // SaveClicked() {
    //     console.log(`[] save clicked`, this.severities)
    //     this.$emit('filterUpdated', this.severities);
    // }
}


