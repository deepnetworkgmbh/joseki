import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';

import { DataService } from '@/services/';
import { InfrastructureComponent } from '@/models';
import { CheckResultSet } from '@/models/CheckResultSet';
import { DateTime } from 'luxon';
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

    resizedFinished: any;
    windowWidth: number = 0;
    windowHeight: number = 0;
    pageSize: number = 0;
    pageIndex: number = 0;

    headers: TableColumn[] = [
        new TableColumn('Component', 'component', 11, 'left'),
        new TableColumn('Category', 'category', 8, 'left'),
        new TableColumn('Collection', 'collection', 22, 'left'),
        new TableColumn('Resource', 'resource', 15, 'left'),
        new TableColumn('Owner', 'owner', 9, 'left'),
        new TableColumn('Control', 'control', 27, 'left'),
        new TableColumn('Result', 'result', 8, 'right')
    ]

    /**
     * Adjust header widths on start,
     * add event listener for resize.
     *
     * @memberof OverviewDetail
     */
    created() {
        this.adjustHeaderWidths();
        window.addEventListener('resize', this.onResize);
        this.onResize();
    }

    /**
     * Adjust header widths on mounted.
     *
     * @memberof OverviewDetail
     */
    mounted() {
        this.adjustHeaderWidths();
    }

    /**
     * Remove event listener for resize on destroy.
     *
     * @memberof OverviewDetail
     */
    beforeDestroy() {
        window.removeEventListener('resize', this.onResize);
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
                    this.data = <CheckResultSet>response;
                    let component = new InfrastructureComponent();
                    component.category = 'Overall';
                    component.name = 'Scan Details'
                    this.$emit('dateChanged', this.selectedDate!.toISODate())
                    this.$emit('componentChanged', component)
                    this.loaded = true;
                    this.adjustHeaderWidths();
                    this.$forceUpdate();
                }
            })
            .catch((error) => {
                //console.log(error);
                this.loadFailed = true;
            });
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

    /**
     * Make sure the resize is finished.
     *
     * @memberof OverviewDetail
     */
    onResize() {
        clearTimeout(this.resizedFinished);
        this.resizedFinished = setTimeout(this.handleResize, 150);
    }

    /**
     * Handle page resize using window height.
     *
     * @memberof OverviewDetail
     */
    handleResize() {
        this.windowHeight = window.innerHeight
        this.pageSize = Math.floor((this.windowHeight - 180) / 22);
        let maxPageCount = Math.floor(this.data.totalResults / this.pageSize);
        if (this.pageIndex > maxPageCount) {
            this.pageIndex = maxPageCount;
        }
        this.adjustHeaderWidths();
    }

    /**
     * Adjust header widths using the current client width.
     *
     * @memberof OverviewDetail
     */
    adjustHeaderWidths() {
        let navElement = document.getElementById('nav');
        if (!navElement) return;
        this.windowWidth = navElement.clientWidth;
        for (let i = 0; i < this.headers.length; i++) {
            this.headers[i].width = Math.floor(this.windowWidth * this.headers[i].percentage / 100) * 1.3
        }
    }

    /**
     * Handle page index change.
     *
     * @param {number} index
     * @memberof OverviewDetail
     */
    changePageIndex(index: number) {
        this.pageIndex = index;
        this.loadData();
    }

    /**
     * Get sort parameter serialized.
     *
     * @returns {string}
     * @memberof OverviewDetail
     */
    getSortData(): string {
        let result: string[] = [];
        for (let i = 0; i < this.headers.length; i++) {
            if (this.headers[i].sortable === false) continue;
            if (this.headers[i].sort === Sorting.none) continue;
            let symbol = (this.headers[i].sort === Sorting.up) ? '-' : '+';
            result.push(`${symbol}${this.headers[i].tag}`)
        }
        if (result.length === 0) return btoa('');
        return btoa(result.join(","));
    }

    /**
     * Handle sorting using selected column index.
     *
     * @param {number} index
     * @memberof OverviewDetail
     */
    changeOrdering(index: number) {
        for (let i = 0; i < this.headers.length; i++) {
            if (i === index) {
                switch (this.headers[index].sort) {
                    case 'UP':
                        this.headers[index].sort = Sorting.none; break;
                    case 'DOWN':
                        this.headers[index].sort = Sorting.up; break;
                    case 'NONE':
                        this.headers[index].sort = Sorting.down; break;
                }
            } else {
                this.headers[i].sort = Sorting.none;
            }
        }
        router.push(`/overview-detail/${this.date}/${this.filter}/${this.getSortData()}`)
    }

    /**
     * Returns the component icon using category.
     *
     * @param {string} category
     * @returns
     * @memberof OverviewDetail
     */
    getComponentIcon(category: string) {
        if (category === 'Subscription') {
            return 'icon-azuredevops';
        }
        if (category === 'Kubernetes') {
            return 'icon-kubernetes';
        }
        return ''
    }

    /**
     * Returns class for sorting indicator.
     *
     * @param {number} index
     * @returns
     * @memberof OverviewDetail
     */
    getHeaderClass(index: number) {
        let header = this.headers[index];
        switch (header.sort) {
            case 'DOWN': return 'icon-chevron-down';
            case 'UP': return 'icon-chevron-up';
            case 'NONE': return 'icon-minus';
        }
    }

    /**
     * Return inline width and text align style using header data.
     *
     * @param {number} index
     * @returns
     * @memberof OverviewDetail
     */
    getColumnWidth(index: number) {
        return {
            maxWidth: this.headers[index].width + 'px',
            textAlign: this.headers[index].textAlign
        };
    }

    /**
     * Handle changes on pageSize.
     *
     * @private
     * @param {number} newValue
     * @memberof OverviewDetail
     */
    @Watch('pageSize')
    private onPageSizeChanged(newValue: number) {
        this.loadData();
    }

    /**
     * Handle changes on sorting.
     *
     * @private
     * @param {string} newValue
     * @memberof OverviewDetail
     */
    @Watch('sort')
    private onSortChanged(newValue: string) {
        this.loadData();
    }

    /**
     * Handle changes on column filtering.
     *
     * @private
     * @param {string} newValue
     * @memberof OverviewDetail
     */
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
    constructor(public label: string, public tag: string, public percentage: number = 0, public textAlign: string = 'left') { }

    checkedCount() {
        return this.options.filter(x => x.checked === true).length;
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