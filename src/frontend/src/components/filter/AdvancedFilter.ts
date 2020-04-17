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
    addMenuX: number = 0;

    selectedFilterType?: string = ''
    selectedFilterValue?: string = ''
    
    addFilterTypes: string[] = [];   
    addFilterValues: CheckLabel[] = [];
    addFilterSelectionCount: number = 0;
    addFilterSelection: string[] = [];

    addFilterButtonEnabled = false;
    filteredValueFilter: string = '';
    onlyWithValues: boolean = false;

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

    @Watch('addFilterValues', { immediate:true, deep: true })
    onAddFilterValuesChanged(newValue: CheckLabel[]) {
        this.addFilterSelectionCount = newValue.filter(x=>x.checked === true).length;
    }

    @Watch('selectedFilterType')
    selectedFilterTypeChanged(newValue) {
        if(this.selectedFilterType) {
            this.resetAddFilterValues();
            this.addFilterValues = this.headerData[this.selectedFilterType]
                                       .map(x=> new CheckLabel(x.name, this.filterContainer!.isInFilter(this.selectedFilterType!, x.name), x.count));
        }
    }

    addSelectionToFilters() {
        if (this.selectedFilterType) {
            this.getFilteredFilterValues()
            .filter(x=>x.checked === true)
            .forEach(cl=> this.filterContainer!.addFilter(this.selectedFilterType!, cl.label))
        }
        this.hideAddMenu();
        this.$emit('filterUpdated', this.filterContainer!.getFilterString());    
    }

    resetAddFilterValues() {
        //this.selectedFilterType = '';
        //this.selectedFilterValue = "No value selected";
        //this.addFilterValues = [];
    }

    toggleFilterValueChecked(index: number) {
        this.getFilteredFilterValues()[index].checked = !this.getFilteredFilterValues()[index].checked;


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
            this.hideAddMenu();
        }
    }
 

    loadData(currentFilter: string, callback?: Function, omitPrevious = false) {
        this.service
            .getGeneralOverviewSearch(DateTime.fromISO(this.$route.params.date), currentFilter)  //
            .then(newHeaderData => {
                if (newHeaderData) {   
                    this.headerData = newHeaderData;
                    let currentFilterTypes = this.filterContainer!.filters.map(x=> x.label);
                    this.addFilterTypes = omitPrevious ? Object.keys(newHeaderData).filter(x=> currentFilterTypes.indexOf(x) === -1)
                                                       : Object.keys(newHeaderData);
                    if(callback) {
                        callback();
                    }
                }
            });
    }

    showMenuInAddMode() {
        this.getMenuXPosition(-1);
        let filterString = this.filterContainer!.getFilterString();
        this.addFilterValues = [];
        this.loadData(filterString, () => {
            this.mode = 'add';
            this.showAddMenu = true;
            this.selectedFilterType = '';
        }, true)
    }

    showMenuInEditMode(index: number) {
        this.getMenuXPosition(index);
        let filter = this.filterContainer!.filters[index];
        let filterString = this.filterContainer!.getFilterString(index);
        this.loadData(filterString, () => {
            this.mode = 'edit';
            this.showAddMenu = true;
            this.selectedFilterType = filter.label;
        }, false)
    }

    hideAddMenu() {
        this.selectedFilterType = ''
        this.selectedFilterValue = ''
        this.showAddMenu = false;
        this.filteredValueFilter = '';
        this.showAddMenu = false;
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

    getFilteredFilterValues() {
        if(this.onlyWithValues) {
            return this.addFilterValues
            .slice()
            .filter(x=> x.count>0 && x.label.toLowerCase().indexOf(this.filteredValueFilter.trim().toLowerCase()) > -1);

        }
        return this.addFilterValues
                   .slice()
                   .filter(x=> x.label.toLowerCase().indexOf(this.filteredValueFilter.trim().toLowerCase()) > -1);
    }

    getHighlightedText(txt: string) {
        return txt.replace(new RegExp(this.filteredValueFilter, "gi"), match => {
            return '<span class="filter-value-highlight">' + match + '</span>';
        });
    }

    getMenuXPosition(filterIndex: number) {
        const elementId = (filterIndex === -1) ? 'add-filter-button' : `edit-filter-button-${filterIndex}`;
        let element = document.getElementById(elementId);
        this.addMenuX = Math.round(element!.getBoundingClientRect().left)-400;
        if (this.addMenuX > 430) { this.addMenuX = 430 }
    }

    getFilterTypeClass(option : string) {
        if (this.mode === 'edit') {
            return option === this.selectedFilterType ? 'adv-filter-selected' : 'adv-filter-hidden'
        }
        return option === this.selectedFilterType ? 'adv-filter-selected' : ''        
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