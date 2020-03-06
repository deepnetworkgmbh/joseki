import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import InfComponent from '@/components/component/InfComponent.vue';

import { InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponent } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';

@Component({
    components: { Spinner, StatusBar, InfComponent }
})
export default class Overview extends Vue {

    @Prop({ default: null })
    date!: string;

    selectedDate?: Date = new Date();

    selectedDat
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureOverview = new InfrastructureOverview();
    panelOpen: boolean = false;
    checkedScans: any[] = [];

    loadData() {
        let dateString = (this.date === null) ? '' : this.date;

        this.service
            .getGeneralOverviewData(dateString)
            .then(response => {
                if (response && this.data.components && this.data.overall) {
                    this.data = response;
                    this.loaded = true;
                    this.setupCharts();
                }
            });
    }

    created() {
        window.addEventListener("resize", this.setupCharts);
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
            : this.data.overall.scoreHistory[0].recordedAt;

        ChartService.drawPieChart(this.data.overall.current, "overall_pie", 300)
        ChartService.drawBarChart(this.data.overall.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked, 100, undefined, 4)
        for (let i = 0; i < this.data.components.length; i++) {
            ChartService.drawBarChart(this.data.components[i].scoreHistory, 'bar' + i, this.selectedDate, this.goComponentDetail, 52, '', 0, this.data.components[i].component.id);
        }
    }

    dayClicked(date: Date) {
        router.push('/overview/' + encodeURIComponent(date.toDateString()));
    }

    goComponentDetail(date: Date, componentId: string) {
        console.log(`[] go component detail`, componentId, date);
        router.push('/component-detail/' + componentId + '/' + encodeURIComponent(date.toDateString()));
    }

    goComponentHistory(component: InfrastructureComponent) {
        if (component) {
            router.push('/component-history/' + component.id);
        } else {
            router.push('/component-history/');
        }
    }

    getArrowHtml(i: number) {
        const scans = this.data.overall.scoreHistory;
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
        return this.data.overall.scoreHistory.slice(0, 5);
    }

    getClusters() { return this.data.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions() { return this.data.components.filter(x => x.component.category === 'Azure Subscription').length; }


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

    @Watch('date', { immediate: true })
    private onDateChanged(newValue: Date) {
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }

}