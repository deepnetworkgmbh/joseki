import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { KubeOverview } from '@/models';
import { ViewMode } from '@/types/Enums';
import { ScanSummary } from '@/models/ScanSummary';
import { InfrastructureOverview, InfrastructureComponentSummary } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';

@Component({
    components: { Spinner, StatusBar }
})
export default class Overview extends Vue {

    @Prop({ default: null })
    date!: string;

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureOverview = new InfrastructureOverview();
    viewMode: ViewMode = ViewMode.detailed;
    grade: string = '?';
    score: number = 0;

    created() {
        window.addEventListener("resize", this.setupCharts);
    }

    loadData() {
        let dateString = (this.date === null) ? '' : this.date;
        this.service.getGeneralOverviewData(dateString)
            .then(response => {
                this.data = response;
                this.data.overall.scoreHistory = this.data.overall.scoreHistory.reverse();
                this.score = this.data.overall.current.passed;
                this.grade = ScoreService.getGrade(this.score);
                if (this.data.components && this.data.overall) {
                    //console.log(`[] data is`, this.data);
                    this.loaded = true;
                    this.setupCharts();
                }
            });
    }


    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        for (let i = 0; i < this.data.components.length; i++) {
            // ugly fix, getter does not work
            this.data.components[i].sections = InfrastructureComponentSummary.getSections(this.data.components[i].current);
        }
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }


    drawCharts() {
        let _date;
        if (this.date === null) {
            _date = this.data.overall.scoreHistory[0].recordedAt;
            this.date = _date;
        } else {
            _date = new Date(decodeURIComponent(this.date));
        }

        ChartService.drawPieChart(this.data.overall.current, "overall_pie", 300)
        ChartService.drawBarChart(this.data.overall.scoreHistory, "overall_bar", _date, this.dayClicked)
        for (let i = 0; i < this.data.components.length; i++) {
            ChartService.drawBarChart(this.data.components[i].scoreHistory, 'bar' + i, _date, this.dayClicked, 48);
        }
    }

    dayClicked(date: Date) {
        //console.log(`[] clicked ${date.toISOString()}`)
        router.replace('/overview/' + encodeURIComponent(date.toISOString()));
    }

    getViewModeClass(index: number) {
        let result = "btn ";
        if (index === 0 && this.viewMode === ViewMode.list ||
            index === 1 && this.viewMode === ViewMode.detailed
        ) {
            result += 'btn-selected';
        }
        return result;
    }

    getScoreIconClass(): string {
        let result = '';
        const score = this.data.overall.current.score;
        this.grade = ScoreService.getGrade(score);

        if (score > 0 && score <= 25) {
            result = "fa fa-poo-storm";
        }
        if (score > 25 && score <= 50) {
            result = "fa fa-cloud-rain"
        }
        if (score > 50 && score <= 75) {
            result = "fa fa-cloud-sun"
        }
        if (score > 75) {
            result = "fa fa-sun";
        }
        return result;
    }

    getArrowHtml(i: number) {
        const scans = this.data.overall.scoreHistory;
        if (i > scans.length) return;
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

    getClusters() { return this.data.components.filter(x => x.category === 'Kubernetes').length; }
    getSubscriptions() { return this.data.components.filter(x => x.category === 'Subscription').length; }

    setViewMode(vm: ViewMode) {
        this.viewMode = vm;
    }

    @Watch('date', { immediate: true })
    private onApplianceStatesLoaded(newValue: Date) {
        this.loadData();
    }
}