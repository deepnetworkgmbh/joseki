import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import InfComponent from '@/components/component/InfComponent.vue';
import { InfrastructureOverview, InfrastructureComponent, ScoreHistoryItem } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { DateTime } from 'luxon';

@Component({
    components: { Spinner, StatusBar, InfComponent }
})
export default class Overview extends Vue {

    @Prop({ default: null })
    date!: string;

    selectedScore: number = 0;
    selectedDate?: DateTime = undefined;
    loaded: boolean = false;
    service: DataService = new DataService();
    data!: InfrastructureOverview;

    loadData() {
        this.selectedDate = (this.date === null) ? undefined : DateTime.fromISO(this.date);
        this.service
            .getGeneralOverviewData(this.selectedDate)
            .then(response => {
                if (response) {
                    this.data = response;
                    let index = this.data.overall.scoreHistory.findIndex(x=>x.recordedAt.startsWith(this.date));
                    if(index<0) { index = 0; }
                    this.selectedDate = DateTime.fromISO(this.data.overall.scoreHistory[index].recordedAt);
                    this.selectedScore = this.data.overall.scoreHistory[index].score;
                    this.$emit('dateChanged', this.selectedDate.toISODate())
                    this.$emit('componentChanged', this.data.overall.component)
                    this.loaded = true;
                    this.$forceUpdate();
                }
            });
    }

    getAreaSeries() {
        return [{ data: this.data.overall.scoreHistory.map((item)=> ({ x: item.recordedAt.split('T')[0] , y: item.score })).reverse() }]
    }
    
    getAreaChartOptions() : ApexCharts.ApexOptions {
        return ChartService.AreaChartOptions("overviewchart", this.data.overall.scoreHistory, [this.selectedDate!], [this.selectedScore], this.dayClicked);
    }

    getPieChartSeries() {
        return this.data.overall.current.getSeries()
    }

    getPieChartOptions() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall", this.data.overall.current)
    }
  
    dayClicked(date: string) {
        this.selectedDate = DateTime.fromISO(date);
        router.push('/overview/' + date);
        this.$forceUpdate();
    }

    goComponentHistory(component: InfrastructureComponent) {
        if (component) {
            router.push('/component-history/' + component.id);
        } else {
            router.push('/component-history/');
        }
    }

    get shortHistory() { return this.data.overall.scoreHistory.slice(0, 5); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
    getClusters() { return this.data.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions() { return this.data.components.filter(x => x.component.category === 'Azure Subscription').length; }
    getHistoryClass(scan: ScoreHistoryItem) {
        return scan.recordedAt.startsWith(this.selectedDate!.toISODate()) ? 'history-selected' : 'history';
    }

    @Watch('date', { immediate: true })
    private onDateChanged(newValue: string) {
        this.selectedDate = DateTime.fromISO(newValue);
        this.$emit('dateChanged', this.selectedDate.toISODate())
        this.loadData();
    }
}