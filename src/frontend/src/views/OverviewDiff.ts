import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';

import { DataService, ScoreService, ChartService } from '@/services/';
import { InfrastructureOverviewDiff, InfrastructureComponent, InfrastructureOverview, CountersSummary } from '@/models';

/**
 * Overview Diff is for comparing the overview scan for two different dates
 *
 * @export
 * @class OverviewDiff
 * @extends {Vue}
 */
@Component
export default class OverviewDiff extends Vue {

    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    loaded: boolean = false;
    loadFailed: boolean = false;
    service: DataService = new DataService();
    data!: InfrastructureOverviewDiff;
    checkedScans: any[] = [];

    /**
     * Load data when view is created
     *
     * @memberof OverviewDiff
     */
    created() {
        this.loadData();
    }

    /**
     * Make an api call for getting the general overview diff data
     * betweed dates date and date2
     * @memberof OverviewDiff
     */
    loadData() {
        this.service
            .getGeneralOverviewDiffData(this.date, this.date2)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.loaded = true;
                }
            })
            .catch(()=> { this.loadFailed = true; });
           
    }
    
    /**
     * Navigates to component-diff page
     * using component id , date1 and date2
     *
     * @param {InfrastructureComponent} component
     * @memberof OverviewDiff
     */
    goDiffPage(component: InfrastructureComponent) {
        router.push('/component-diff/' + component.id + '/' + this.date + '/' + this.date2);
    }

    /**
     * Gets the pie counters from summary using componentId
     *
     * @param {InfrastructureOverview} summary
     * @param {string} componentId
     * @returns {(CountersSummary | undefined)}
     * @memberof OverviewDiff
     */
    getCurrentFromSummary(summary: InfrastructureOverview, componentId: string) : CountersSummary | undefined {
        const index = summary.components.findIndex(x=>x.component.id === componentId);
        if(index === -1) {
            return undefined;
        }
        return summary.components[index].current;
    }

    /**
     * Series data for pie1
     *
     * @memberof OverviewDiff
     */
    getPieChartSeries1() { return this.data.summary1.overall.current.getSeries() }

    /**
     * Series data for pie2
     *
     * @memberof OverviewDiff
     */
    getPieChartSeries2() { return this.data.summary2.overall.current.getSeries() }

    /**
     * Chart options for pie1
     *
     * @memberof OverviewDiff
     */
    getPieChartOptions1() { return ChartService.PieChartOptions("pie-overall1", this.data.summary1.overall.current, ()=>{}, true) }

    /**
     * Chart options for pie2
     *
     * @memberof OverviewDiff
     */
    getPieChartOptions2() { return ChartService.PieChartOptions("pie-overall2", this.data.summary2.overall.current, ()=>{}, true) }
        
    /**
     * Number of clusters on scan 1
     *
     * @memberof OverviewDiff
     */
    getClusters1() { return this.data.summary1.components.filter(x => x.component.category === 'Kubernetes').length }

    /**
     * Number of clusters on scan 2
     *
     * @memberof OverviewDiff
     */
    getClusters2() { return this.data.summary2.components.filter(x => x.component.category === 'Kubernetes').length }

    /**
     * Subscriptions of clusters on scan 1
     *
     * @memberof OverviewDiff
     */
    getSubscriptions2() { return this.data.summary2.components.filter(x => x.component.category === 'Azure Subscription').length }

    /**
     * Subscriptions of clusters on scan 2
     *
     * @memberof OverviewDiff
     */
    getSubscriptions1() { return this.data.summary1.components.filter(x => x.component.category === 'Azure Subscription').length }

    /**
     * Calculates grade using score
     *
     * @memberof OverviewDiff
     */
    getGrade(score: number) { return ScoreService.getGrade(score) }
}