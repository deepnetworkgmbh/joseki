import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { DataService, ScoreService, MappingService, ChartService } from '@/services/';
import { InfrastructureComponentSummary, ScoreHistoryItem, SeverityFilter, InfrastructureComponent } from '@/models';
import { CheckCollection, CheckControlGroup } from '@/services/DiffService';
import { FilterContainer } from '@/models/FilterContailer';
import { CheckResultSet } from '@/models/CheckResultSet';
import { OverviewCheck } from '@/models/Check';

@Component
export default class ComponentDetail extends Vue {

    @Prop()
    id!: string;

    @Prop({ default: null })
    date!: string;

  
    filter: string = '';
    filterContainer!: FilterContainer;
    
    selectedDate?: DateTime;
    selectedScore: number = 0;
    loaded: boolean = false;
    loadFailed: boolean = false;
    service: DataService = new DataService();

    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    checkResultSet: CheckResultSet = new CheckResultSet();

    resultsByCollection: CheckCollection[] = [];
    allExpanded: boolean = false;
    previousTop: number = 0;

    /**
     * make an api call and load Component detail data
     * TODO: first request returns redundant check list, must be removed.
     * @memberof ComponentDetail
     */
    loadData() {
        this.selectedDate = (this.date === null) ? undefined : DateTime.fromISO(this.date);
        this.service
            .getComponentDetailData(decodeURIComponent(this.id), this.selectedDate)
            .then(response => {
                if (response) {          
                    this.data = response;
                    let index = this.data.scoreHistory.findIndex(x=>x.recordedAt.startsWith(this.date));
                    if(index<0) { index = 0; }
                    this.selectedDate = DateTime.fromISO(this.data.scoreHistory[index].recordedAt);
                    this.selectedScore = this.data.scoreHistory[index].score;
                    this.$emit('dateChanged', this.selectedDate.toISODate())
                    this.$emit('componentChanged', this.data.component)
                    this.filter = btoa(`component=${this.data.component.name}`);
                    this.filterContainer = new FilterContainer('component', this.filter);
                }
                return this.service;
            })
            .then((service) => service.getGeneralOverviewDetail(0, 0, this.selectedDate, this.filter))           
            .then(response => {
                if (response) {
                    this.checkResultSet = <CheckResultSet>response;
                    this.resultsByCollection =  MappingService.getResultsByCollection(this.checkResultSet.checks);
                    console.log(JSON.parse(JSON.stringify(this.resultsByCollection)));
                    this.loaded = true;
                    this.$forceUpdate();
                }
            })
            .catch(()=> { this.loadFailed = true; });
    }

    toggleCollectionChecked(index: string) {
        // console.log(`[Y] ${this.getScrollTop()}`)
        this.previousTop = this.getScrollTop();        
        this.resultsByCollection[index].checked = !this.resultsByCollection[index].checked;
        //const index = this.resultsByCollection.findIndex(x => x._id === _id);
        //this.resultsByCollection[index].checked = !this.resultsByCollection[index].checked;        
    }

    toggleObjectChecked(index: number, objectIndex: number) {
        this.previousTop = this.getScrollTop();        
        //const objectIndex = this.resultsByCollection[index].objects.findIndex(x => x._id === _id);
        this.resultsByCollection[index].objects[objectIndex].checked = 
        !this.resultsByCollection[index].objects[objectIndex].checked;
    }

    toggleExpand(lvl1: boolean, lvl2: boolean) {
        this.resultsByCollection.forEach(element => {
            element.checked = lvl1;
            element.objects.forEach(obj => {
                obj.checked = lvl2;
            });
        });
    }
    

    /**
     * return series for area chart
     *
     * @returns
     * @memberof ComponentDetail
     */
    getAreaSeries() {
        return ScoreHistoryItem.getInterpolatedThresholdSeries(this.data.scoreHistory);
    }

    /**
     * return options for area chart
     *
     * @returns {ApexCharts.ApexOptions}
     * @memberof ComponentDetail
     */
    getAreaChartOptions() : ApexCharts.ApexOptions {
        return ChartService.AreaChartOptions("overviewchart", this.data.scoreHistory, [this.selectedDate!], [this.selectedScore], this.dayClicked);
    }

    /**
     * return series for pie chart
     *
     * @returns
     * @memberof ComponentDetail
     */
    getPieChartSeries() {
        return this.data.current.getSeries()
    }

    /**
     * return options for pie chart
     *
     * @returns {ApexCharts.ApexOptions}
     * @memberof ComponentDetail
     */
    getPieChartOptions() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall", this.data.current, this.pieClicked)
    }

    pieClicked(status: string) {
        let filterBy = btoa(`result=${status}&component=${this.data.component.name}`);
        router.push(`/overview-detail/${this.selectedDate!.toISODate()}/${filterBy}`); 
    }

    /**
     * handle click event on area chart
     * navigates to selected date
     *
     * @param {string} date
     * @param {string} component
     * @memberof ComponentDetail
     */
    dayClicked(date: string, component: string) {
        router.push('/component-detail/' + encodeURIComponent(this.data.component.id) + '/' + date);
    }

    /**
     * navigates to component history 
     *
     * @memberof ComponentDetail
     */
    goComponentHistory() {
        if (this.data.component) {
            router.push('/component-history/' + this.data.component.id);
        } else {
            router.push('/component-history/');
        }
    }

    /**
     * returns the url for image scan
     *
     * @param {string} imageTag
     * @returns
     * @memberof ComponentDetail
     */
    imageScanUrl(imageTag: string) {
        if(this.selectedDate === undefined) {
            this.selectedDate = DateTime.fromISO(this.data.scoreHistory[0].recordedAt);
        } 
        return '/image-detail/' + encodeURIComponent(imageTag) + '/' + this.selectedDate.toISODate();    
    }

    /**
     * returns short list of scans
     *
     * @readonly
     * @memberof ComponentDetail
     */
    get shortHistory() {
        return this.data.scoreHistory.slice(0, 5);
    }

    /**
     * returns results grouped by category
     *
     * @param {InfrastructureComponentSummary} data
     * @returns
     * @memberof ComponentDetail
     */
    getResultsByCategory(data: InfrastructureComponentSummary) { return MappingService.getResultsByCategory(this.data.checks); }

    // /**
    //  * returns results grouped by collection
    //  *
    //  * @param {InfrastructureComponentSummary} data
    //  * @returns
    //  * @memberof ComponentDetail
    //  */
    // getResultsByCollection(checks: OverviewCheck[]) { return MappingService.getResultsByCollection(checks); }

    /**
     * returns grade from score
     *
     * @param {number} score
     * @returns grade
     * @memberof ComponentDetail
     */
    getGrade(score: number) { return ScoreService.getGrade(score); }

    /**
     * returns metadata for category
     *
     * @param {string} category
     * @returns
     * @memberof ComponentDetail
     */
    getCategoryMeta(category: string) {
        let index = this.data.categorySummaries.findIndex(x => x.category === category);
        return (index > -1) ? this.data.categorySummaries[index].description : '';
    }

    /**
     * return class for scan history 
     *
     * @param {ScoreHistoryItem} scan
     * @returns
     * @memberof ComponentDetail
     */
    getHistoryClass(scan: ScoreHistoryItem) {
        if(scan.score === 0) {
            return 'history-not-scanned';
        }
        return scan.recordedAt.startsWith(this.selectedDate!.toISODate()) ? 'history-selected' : 'history';
    }

    updated() {
        // console.log(`previous top: ${this.previousTop}`);
        // console.log(`unwanted top: ${this.getScrollTop()}`);        
        window.scrollTo(0, this.previousTop);
    }

    private getScrollTop() {       
        return window.scrollY;
    }

    onFilterChangedFromAF(filter: string) {
        this.previousTop = this.getScrollTop();
        this.filterContainer = new FilterContainer('component', filter);
        this.filter = filter;
        this.loadFailed = false;
        this.service.getGeneralOverviewDetail(0, 0, this.selectedDate, this.filter)
            .then(response => {
                if (response) {
                    console.log(`[222]`);
                    this.checkResultSet = <CheckResultSet>response;
                    this.resultsByCollection =  MappingService.getResultsByCollection(this.checkResultSet.checks);
                    console.log(JSON.parse(JSON.stringify(this.resultsByCollection)));
                    this.loaded = true;

                    const flen = this.filterContainer.filters.length;
                    this.toggleExpand(true, flen > 1);
       
                    this.$forceUpdate();
                }
            })
            .catch(()=> { this.loadFailed = true; });
    }


    /**
     * Watcher for date, emits dateChanged for breadcrumbs and loads data
     *
     * @private
     * @param {string} newValue
     * @memberof ComponentDetail
     */
    @Watch('date', { immediate: true })
    private onDateChanged(newValue: string) {
        this.selectedDate = DateTime.fromISO(newValue);
        this.$emit('dateChanged', this.selectedDate.toISODate())
        this.loadData();
    }

}