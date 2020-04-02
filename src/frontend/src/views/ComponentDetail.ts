import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import Score from "@/components/score/Score.vue";
import ResultFilter from "@/components/filter/ResultFilter.vue";

import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, ScoreHistoryItem } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { MappingService } from '@/services/MappingService';
import { DateTime } from 'luxon';
import { SeverityFilter } from '@/models/SeverityFilter';

@Component({
    components: { Spinner, StatusBar, Score, ResultFilter }
})
export default class ComponentDetail extends Vue {

    @Prop()
    id!: string;

    @Prop({ default: null })
    date!: string;

    severityFilter: SeverityFilter = new SeverityFilter();
    selectedDate?: DateTime;
    selectedScore: number = 0;
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();

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
                    this.loaded = true;
                    this.$forceUpdate();
                }
            });
    }

    getAreaSeries() {
        return [{ data: this.data.scoreHistory.map((item)=> ({ x: item.recordedAt.split('T')[0] , y: item.score })).reverse() }]
    }
    
    getAreaChartOptions() : ApexCharts.ApexOptions {
        return ChartService.AreaChartOptions("overviewchart", this.data.scoreHistory, [this.selectedDate!], [this.selectedScore], this.dayClicked);
    }

    getPieChartSeries() {
        return this.data.current.getSeries()
    }

    getPieChartOptions() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall", this.data.current)
    }

    dayClicked(date: string, component: string) {
        router.push('/component-detail/' + encodeURIComponent(this.data.component.id) + '/' + date);
    }

    goComponentHistory() {
        if (this.data.component) {
            router.push('/component-history/' + this.data.component.id);
        } else {
            router.push('/component-history/');
        }
    }

    imageScanUrl(imageTag: string) {
        if(this.selectedDate === undefined) {
            this.selectedDate = DateTime.fromISO(this.data.scoreHistory[0].recordedAt);
        } 
        return '/image-detail/' + encodeURIComponent(imageTag) + '/' + this.selectedDate.toISODate();    
    }

    get shortHistory() {
        return this.data.scoreHistory.slice(0, 5);
    }

    @Watch('date', { immediate: true })
    private onDateChanged(newValue: string) {
        this.selectedDate = DateTime.fromISO(newValue);
        this.$emit('dateChanged', this.selectedDate.toISODate())
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
    getResultsByCategory(data: InfrastructureComponentSummary) { return MappingService.getResultsByCategory(this.data.checks); }
    getResultsByCollection(data: InfrastructureComponentSummary) { return MappingService.getResultsByCollection(data.checks, this.severityFilter); }

    getCategoryMeta(category: string) {
        let index = this.data.categorySummaries.findIndex(x => x.category === category);
        if (index > -1) {
            return this.data.categorySummaries[index].description;
        }
        return '???'
    }
    getHistoryClass(scan: ScoreHistoryItem) {
        if(scan.score === 0) {
            return 'history-not-scanned';
        }
        return scan.recordedAt.startsWith(this.selectedDate!.toISODate()) ? 'history-selected' : 'history';
    }

}