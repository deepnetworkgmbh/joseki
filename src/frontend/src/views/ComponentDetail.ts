import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import Score from "@/components/score/Score.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponent, CountersSummary, CheckSeverity } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { MappingService } from '@/services/MappingService';

@Component({
    components: { Spinner, StatusBar, Score }
})
export default class ComponentDetail extends Vue {

    @Prop()
    id!: string;

    @Prop({ default: null })
    date!: string;

    selectedDate: Date = new Date();

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    panelOpen: boolean = false;
    checkedScans: any[] = [];


    created() {
        window.addEventListener("resize", this.setupCharts);
    }

    loadData() {
        let dateString = (this.date === null) ? '' : this.date;
        this.service.getComponentDetailData(this.id, dateString)
            .then(response => {
                this.data = response;
                console.log(`[] data is`, this.data);
                this.data.scoreHistory = this.data.scoreHistory.reverse();
                this.loaded = true;
                this.setupCharts();
            });
    }


    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }

    drawCharts() {
        this.selectedDate = this.date ?
            new Date(decodeURIComponent(this.date))
            : this.data.scoreHistory[0].recordedAt;
        ChartService.drawPieChart(this.data.current, "overall_pie", 300)
        ChartService.drawBarChart(this.data.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked)
    }

    dayClicked(date: Date) {
        //console.log(`[] clicked ${date.toISOString()}`)
        let params = this.id + '/' + this.selectedDate;
        router.push('/component-detail/' + encodeURIComponent(params));
    }

    goComponentHistory() {
        if (this.data.component) {
            router.push('/component-history/' + this.data.component.id);
        } else {
            router.push('/component-history/');
        }
    }


    getArrowHtml(i: number) {
        const scans = this.data.scoreHistory;
        if (i >= (scans.length - 1)) return '-';
        if (scans[i].score > scans[i + 1].score) {
            return '<i class="fas fa-arrow-up" style="color:green;"></i>'
        } else if (scans[i].score < scans[i + 1].score) {
            return '<i class="fas fa-arrow-down" style="color:red;"></i>'
        }
        return '-'
    }

    getScanRowClass(i: number): string {
        return i % 2 === 0 ? 'bg-gray-100' : 'bg-gray-200';
    }

    get shortHistory() {
        return this.data.scoreHistory.slice(0, 5);
    }

    onHistoryClicked() {
        router.push('/overview-history/');
    }

    getPanelClass() {
        this.$emit(this.panelOpen ? 'sideWindowOpened' : 'sideWindowClosed');
        return this.panelOpen ? 'right-menu-open' : 'right-menu';
    }

    canCompare(): boolean {
        return this.checkedScans.length !== 2;
    }

    checkDisabled(i: number, val: string) {
        return this.checkedScans.length > 1 && this.checkedScans.indexOf(val) === -1
    }

    CompareScans() {
        console.log(`[] comparing ${this.checkedScans}`);
        const params = encodeURIComponent(this.checkedScans[1]) + '/' + encodeURIComponent(this.checkedScans[0]);
        router.push('/overview-diff/' + params);
    }

    @Watch('id', { immediate: true })
    private onDateChanged(newValue: Date) {
        console.log(`[] id changed, loading data`);
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }

    get ResultsByCategory() { return MappingService.getResultsByCategory(this.data.checks); }
    get ResultsByCollection() { return MappingService.getResultsByCollection(this.data.checks); }


}