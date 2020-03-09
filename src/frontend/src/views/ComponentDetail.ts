import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import Score from "@/components/score/Score.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary } from '@/models/InfrastructureOverview';
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

    selectedDate?: Date = new Date(this.date);

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    panelOpen: boolean = false;
    checkedScans: any[] = [];

    created() {
        window.addEventListener("resize", this.setupCharts);
    }

    loadData() {
        console.log(`[] id = `, decodeURIComponent(this.id));
        let dateString = (this.selectedDate === undefined) ? '' : this.selectedDate.toDateString();
        this.service.getComponentDetailData(decodeURIComponent(this.id), dateString)
            .then(response => {
                if (response) {
                    this.data = response;
                    console.log(`[] data is`, this.data);
                    this.setupCharts();
                    this.loaded = true;
                }
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
        if (this.selectedDate === undefined) {
            this.selectedDate = this.date ? new Date(decodeURIComponent(this.date)) : this.data.scoreHistory[0].recordedAt;
        }
        ChartService.drawPieChart(this.data.current, "overall_pie", 300);
        ChartService.drawBarChart(this.data.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked, 100, undefined, 4, this.data.component.id);
    }

    dayClicked(date: Date, component: string) {
        console.log(`[] dayClicked ${date}`)
        this.selectedDate = date;
        router.push('/component-detail/' + encodeURIComponent(component) + '/' + encodeURIComponent(date.toDateString()));
    }

    goComponentHistory() {
        if (this.data.component) {
            router.push('/component-history/' + this.data.component.id);
        } else {
            router.push('/component-history/');
        }
    }

    goToImageScan(imageTag: string) {
        console.log(`[] current date`, this.selectedDate);
        if (this.selectedDate) {
            router.push('/image-detail/' + encodeURIComponent(imageTag) + '/' + encodeURIComponent(this.selectedDate.toDateString()));
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

    @Watch('date', { immediate: true })
    private onDateChanged(newValue: Date) {
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
    getResultsByCategory(data: InfrastructureComponentSummary) { return MappingService.getResultsByCategory(data.checks); }
    getResultsByCollection(data: InfrastructureComponentSummary) { return MappingService.getResultsByCollection(data.checks); }

    getCategoryMeta(category: string) {
        let index = this.data.categorySummaries.findIndex(x => x.category === category);
        if (index > -1) {
            return this.data.categorySummaries[index].description;
        }
        return '???'
    }

}