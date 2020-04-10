import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { SeverityFilter } from '@/models/SeverityFilter';
import { FilterContainer } from '@/models/FilterContailer';

@Component
export default class AdvancedFilter extends Vue {

    @Prop()
    filter!:string;

    menuOpen: boolean = false;
    addFilterOpen: boolean = false;
    filterinput: string = '';

    filterContainer!: FilterContainer;

    deleteFilter(index: number) {
        this.filterContainer.removeFilterByIndex(index);
        this.$emit('filterUpdated', this.filterContainer.getFilterString());
        this.menuOpen = false;
    }

    @Watch('filter', { immediate: true }) 
    onFilterChanged(newValue: string) {
        this.filterContainer = new FilterContainer(this.filter);
        this.$forceUpdate();
    }

    calculateMenuHeight() {
        let height = 20 + this.filterContainer.filters.length * 25 ;
        return `${height}px`;
    } 
}

export class Filter {
    label: string = '';
    values: string[] = [];

    

}