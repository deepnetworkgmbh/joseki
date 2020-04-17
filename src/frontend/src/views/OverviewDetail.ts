import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';

import { DataService, ScoreService, ChartService } from '@/services/';
import { InfrastructureOverviewDiff, InfrastructureComponent, InfrastructureOverview, CountersSummary } from '@/models';
import { CheckResultSet } from '@/models/CheckResultSet';
import { DateTime } from 'luxon';
import { PageButton } from '@/models/PageButton';
import { Filter } from '@/components/filter/AdvancedFilter';
import { FilterContainer } from '@/models/FilterContailer';

/**
 * Overview Detail lists number of scan details 
 * of the overall infrastructure regardless of the component.
 *
 * @export
 * @class OverviewDetail
 * @extends {Vue}
 */
@Component
export default class OverviewDetail extends Vue {

    @Prop({ default: null })
    date!: string;

    @Prop({ default: '' })
    filter!: string;

    @Prop({ default: '' })
    sort!: string;

    loaded: boolean = false;
    loadFailed: boolean = false;
    selectedDate?: DateTime = undefined;
    service: DataService = new DataService();
    data!: CheckResultSet;
    filterContainer!: FilterContainer;

    windowWidth: number = 0;
    windowHeight: number = 0;
    pageSize: number = 0;
    pageIndex: number = 0;
    pageButtons: number[] = [];

    headers: TableColumn[] = [
        new TableColumn('Component', 'component', 11, 'left'),
        new TableColumn('Category', 'category', 8, 'left'),     
        new TableColumn('Collection', 'collection', 24, 'left'),
        new TableColumn('Resource', 'resource', 20, 'left'),    
        new TableColumn('Control', 'control', 29, 'left'),
        new TableColumn('Result', 'result', 8, 'right')
    ]
    headerData: any;

    /**
     * load data when component is created.
     *
     * @memberof OverviewDetail
     */
    created() {
        this.adjustHeaderWidths();
        window.addEventListener('resize', this.onResize);
        this.onResize();
    }

    paintHeaders() {
        if (!this.headerData) return;
        for(let h=0;h< this.headers.length;h++) {
            let header = this.headers[h];                   
            let options = this.headerData[header.tag].map((data) => new FilterCheck(data.name));
            
            // search within options and mark them as checked if exists in the current filter
            for(let j=0;j<this.filterContainer.filters.length; j++) {    
                if (this.filterContainer.filters[j].label !== header.tag) continue;
                for(let i=0;i<options.length; i++) {
                    options[i].checked = (this.filterContainer.filters[j]
                                              .values
                                              .map(x=> x.toLowerCase())
                                              .indexOf(options[i].label.toLowerCase()) !== -1);
                }
            }
            for(let j=0;j<options.length;j++) {
                options[j].dimmed = this.checkIfOptionExistsInFilter(header.tag, options[j]);
            }
            this.headers[h].options = options;
        }
    }
    
    checkIfOptionExistsInFilter(tag: string, filter: FilterCheck): boolean {
        if (this.headerData === undefined) return true;
        if (this.headerData[tag].length === 0) return true;
        const index = this.headerData[tag].findIndex(x=>x.name === filter.label);
        if (index === -1) return true;
        return this.headerData[tag][index].filteredOut;
    }

    /**
     * Make an api call for getting the general overview data
     *
     * @memberof Overview
     */
    loadData() {
        if (this.pageSize === 0) {
            return;
        }
        this.loadFailed = false;
        this.selectedDate = DateTime.fromISO(this.date);
        this.service
            .getGeneralOverviewDetail(this.pageSize, this.pageIndex, this.selectedDate, this.filter, this.sort)
            .then(response => {
                if (response) {
                    this.data = response;            
                    let component = new InfrastructureComponent();
                    component.category = 'Overall';
                    component.name = 'Scan Details'
                    this.$emit('dateChanged', this.selectedDate!.toISODate())
                    this.$emit('componentChanged', component)
                    this.loaded = true;

                    this.service
                        .getGeneralOverviewSearch(this.selectedDate, this.filter)  //
                        .then(newHeaderData => {
                            if (newHeaderData) {   
                                this.headerData = newHeaderData;
                                this.paintHeaders();
                            }
                    });
                    this.adjustHeaderWidths();
                    this.$forceUpdate();
                }
            })
            .catch((error)=> { 
                console.log(error);
                this.loadFailed = true; 
            });        
    }

    toggleColumnFilter(index: number) {
        const column = this.headers[index];
        if (column.optionsMenuShown) {
            column.optionsMenuShown = false;
            return;
        }
        // close other menus
        for(let i=0;i<this.headers.length;i++) {
            this.headers[i].optionsMenuShown = (index === i);            
        }
    }

    toggleFilterSelection(headerIndex: number, rowIndex: number) {
        let header = this.headers[headerIndex];
        let option = header.options[rowIndex];
        if(option.checked) {
            this.addFilter(header.tag, option.label);
        }else{
            this.removeFilter(header.tag, option.label);
        }        
    }

    removeFilter(label: string, value: string) {
        this.filterContainer.removeFilterValue(label, value);
        this.onFilterUpdated(this.filterContainer.getFilterString());
    }

    addFilter(label: string, value: string) {
        this.filterContainer.addFilter(label, value);
        this.onFilterUpdated(this.filterContainer.getFilterString());
    }

    onFilterUpdated(updatedFilter: string) {
        this.filterContainer = new FilterContainer(updatedFilter);
        this.paintHeaders();
        router.push(`/overview-detail/${this.date}/${updatedFilter}/${this.getSortData()}`)  
    }

    /**
     * returns the url for image scan
     *
     * @param {string} imageTag
     * @returns
     * @memberof OverviewDetail
     */
    imageScanUrl(imageTag: string) {
        return '/image-detail/' + encodeURIComponent(imageTag) + '/' + this.selectedDate!.toISODate();    
    }

    beforeDestroy() { 
        window.removeEventListener('resize', this.onResize); 
    }

    onResize() {
        this.windowHeight = window.innerHeight
        this.pageSize = Math.floor((this.windowHeight-180)/22);  
    }

    mounted() {
        this.adjustHeaderWidths();
    }

    adjustHeaderWidths() {
        let footerElement = document.getElementById('header-bar');
        if (footerElement) {
            this.windowWidth = footerElement.clientWidth;
            let sum = 0;
            for(let i=0;i<this.headers.length;i++) {
                this.headers[i].width = Math.floor(this.windowWidth * this.headers[i].percentage / 100) 
                sum+= this.headers[i].width;
            }    
        }
    }

    changePageIndex(index: number) {
        this.pageIndex = index;
        this.loadData();
    }

    getSortData(): string {
        let result: string[] = [];
        for (let i=0; i<this.headers.length; i++) {
            if (this.headers[i].sortable === false) continue;
            if (this.headers[i].sort === Sorting.none) continue;
            let symbol = (this.headers[i].sort === Sorting.up) ? '-' : '+';
            result.push(`${symbol}${this.headers[i].tag}`)
        }
        if (result.length === 0) return btoa('');
        return btoa(result.join(","));
    }

    changeOrdering(index: number) {
        for(let i=0;i<this.headers.length;i++) {
            if (i === index) {
                switch(this.headers[index].sort) {
                    case 'UP': 
                        this.headers[index].sort = Sorting.none; break;
                    case 'DOWN':
                        this.headers[index].sort = Sorting.up; break;
                    case 'NONE':
                        this.headers[index].sort = Sorting.down; break;    
                }        
            }else{
                this.headers[i].sort = Sorting.none;
            }
        }
        router.push(`/overview-detail/${this.date}/${this.filter}/${this.getSortData()}`)
    }


    getComponentIcon(category: string) {
        if(category === 'Subscription') {
          return 'icon-azuredevops';
        }
        if(category === 'Kubernetes') {
          return 'icon-kubernetes';
        }
        return ''
    }

    getHeaderClass(index: number) {
        let header = this.headers[index];
        switch(header.sort) {
            case 'DOWN': return 'icon-chevron-down';
            case 'UP': return 'icon-chevron-up';
            case 'NONE': return 'icon-minus';
        }
    }
   
    getResultClass(result: string) {
        return `result${result}`;
    }

    getColumnWidth(index: number) {
        return { 
            maxWidth: this.headers[index].width + 'px', 
            textAlign: this.headers[index].textAlign 
        };
    }

    @Watch('pageSize')
    private onPageSizeChanged(newValue: number) {
        this.loadData();
    }

    @Watch('sort')
    private onSortChanged(newValue: string) {
        this.loadData();
    }

    @Watch('filter', { immediate: true })
    private onFilterChanged(newValue: string) {
        this.filterContainer = new FilterContainer(newValue);
        this.loadData();
    }
}

export class TableColumn {
    sort: Sorting = Sorting.none
    sortable: boolean = true
    options: FilterCheck[] = []
    optionsMenuShown: boolean = false
    selectAll: boolean = false;
    width: number = 0;
    constructor(public label: string, public tag: string, public percentage: number = 0, public textAlign: string = 'left') {

    }

    checkedCount() {
        return this.options.filter(x=>x.checked === true).length;
    }
}

export class FilterCheck {
    checked: boolean = false;
    dimmed: boolean = true;
    constructor(public label: string) { }
}

export enum Sorting {
    up = 'UP',
    down = 'DOWN',
    none = 'NONE'
}