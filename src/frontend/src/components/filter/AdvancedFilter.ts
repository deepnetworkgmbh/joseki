import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { SeverityFilter } from '@/models/SeverityFilter';
import { FilterContainer } from '@/models/FilterContailer';
import { DataService } from '@/services';
import { DateTime } from 'luxon';
import router from '@/router';

@Component
export default class AdvancedFilter extends Vue {

    @Prop()
    filter!:string;

    filterContainer?: FilterContainer;
    service: DataService = new DataService();
    headerData: any;

    showAddMenu: boolean = false;

    selectedFilterType?: string = ''
    selectedFilterValue?: string = ''
    
    addFilterTypes: string[] = [];   
    addFilterValues: CheckLabel[] = [];

    addFilterButtonEnabled = false;
   

    mode: 'edit' | 'add' = 'add';

    deleteFilter(index: number) {
        if(this.filterContainer) {
            this.filterContainer.removeFilterByIndex(index);
            this.$emit('filterUpdated', this.filterContainer.getFilterString());    
        }
        this.showAddMenu = false;
    }

    getAddMenuClass() {
        return this.showAddMenu ? 'advanced-filter-add-menu-show': 'advanced-filter-add-menu-hide';
    }

    @Watch('filter', { immediate: true }) 
    onFilterChanged(newValue: string) {     
        console.log(`[adv] filter updated ${newValue}`)   
        this.filterContainer = new FilterContainer(newValue);
        this.loadData(this.filterContainer.getFilterString());
        this.$forceUpdate();
    }

    @Watch('showAddMenu')
    onAddMenuChanged(shown: boolean) {
        if(shown === true) {
            document.addEventListener('keyup', this.closeIfEscPressed);
        }else {            
            document.removeEventListener('keyup', this.closeIfEscPressed);
            this.resetAddFilterValues();
        }
    }

    @Watch('selectedFilterType')
    selectedFilterTypeChanged(newValue) {
        console.log(newValue);
        if(this.selectedFilterType) {
            console.log(this.headerData[this.selectedFilterType])
            this.resetAddFilterValues();
            this.addFilterValues = this.headerData[this.selectedFilterType]
                                       //.filter(x=>x.filteredOut === false)
                                       .map(x=> new CheckLabel(x.name, this.filterContainer!.isInFilter(this.selectedFilterType!, x.name), x.count));
            console.log(this.addFilterValues)
        }
    }

    addSelectionToFilters() {
        if (this.selectedFilterType) {
            this.addFilterValues
            .filter(x=>x.checked === true)
            .forEach(cl=> this.filterContainer!.addFilter(this.selectedFilterType!, cl.label))
        }
        this.selectedFilterType = ''
        this.selectedFilterValue = ''
        this.showAddMenu = false;
        this.$emit('filterUpdated', this.filterContainer!.getFilterString());    
    }

    resetAddFilterValues() {
        //this.selectedFilterType = '';
        //this.selectedFilterValue = "No value selected";
        //this.addFilterValues = [];
    }

    toggleFilterValueChecked(index: number) {
        this.addFilterValues[index].checked = !this.addFilterValues[index].checked;
        let checkedCount = this.addFilterValues.filter(x=>x.checked === true).length;
        this.selectedFilterValue = (checkedCount === 0) ? "No value selected" : checkedCount + " values selected";
        this.addFilterButtonEnabled = checkedCount > 0;        
    }

    changeFilterType(option) {
        if (this.mode === 'add') {
            this.selectedFilterType = option;
        }
    }

    closeIfEscPressed(event) {
        if (event.keyCode === 27) {
            this.showAddMenu = false;
        }
    }

    loadData(currentFilter: string, callback?: Function, omitPrevious = false) {
        this.service
            .getGeneralOverviewSearch(DateTime.fromISO(this.$route.params.date), currentFilter)  //
            .then(newHeaderData => {
                if (newHeaderData) {   
                    this.headerData = newHeaderData;
                    console.log(`[headerData]`, JSON.parse(JSON.stringify(this.headerData)));
                    //this.paintHeaders();
                    let currentFilterTypes = this.filterContainer!.filters.map(x=> x.label);
                    this.addFilterTypes = omitPrevious ? Object.keys(newHeaderData).filter(x=> currentFilterTypes.indexOf(x) === -1)
                                                       : Object.keys(newHeaderData);

                    console.log(`[] current filter types`, currentFilterTypes);
                    console.log(`[] add filter types`, this.addFilterTypes);
                    if(callback) {
                        callback();
                    }
                }
            });
    }

    showMenuInAddMode() {
        let filterString = this.filterContainer!.getFilterString();
        this.addFilterValues = [];
        this.loadData(filterString, () => {
            this.mode = 'add';
            this.showAddMenu = true;
            this.selectedFilterType = '';
        }, true)
    }

    showMenuInEditMode(index: number) {
        let filter = this.filterContainer!.filters[index];
        console.log(filter.label)
        let filterString = this.filterContainer!.getFilterString(index);
        this.loadData(filterString, () => {
            console.log(`[] editing filter ${index}`);        
            this.mode = 'edit';
            this.showAddMenu = true;
            this.selectedFilterType = filter.label;
        }, false)
    }

    updateFiltersSelection() {
        this.filterContainer!.removeFilterLabel(this.selectedFilterType!);
        this.addFilterValues.filter(x=>x.checked === true)
                            .forEach(cl=> this.filterContainer!.addFilter(this.selectedFilterType!, cl.label))
        
        this.selectedFilterType = ''
        this.selectedFilterValue = ''
        this.showAddMenu = false;
        this.$emit('filterUpdated', this.filterContainer!.getFilterString());    
    }

}

export class Filter {
    label: string = '';
    values: string[] = [];
}

export class CheckLabel {
    label: string = '';
    count: number = 0;
    checked: boolean = false;
    constructor(_label: string, _checked: boolean, _count: number) {
        this.label = _label;
        this.checked = _checked;
        this.count = _count;
    }
}