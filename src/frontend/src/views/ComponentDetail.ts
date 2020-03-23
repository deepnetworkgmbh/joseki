import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import Score from "@/components/score/Score.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { MappingService } from '@/services/MappingService';
import { DateTime } from 'luxon';

@Component({
    components: { Spinner, StatusBar, Score }
})
export default class ComponentDetail extends Vue {

    @Prop()
    id!: string;

    @Prop({ default: null })
    date!: string;

    selectedDate?: DateTime;
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    panelOpen: boolean = false;
    checkedScans: any[] = [];

    loadData() {
        this.selectedDate = (this.date === null) ? undefined : DateTime.fromISO(this.date);

        this.service
            .getComponentDetailData(decodeURIComponent(this.id), this.selectedDate)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.setupCharts();
                    this.loaded = true;
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
        if(this.selectedDate === undefined) {
            this.selectedDate = DateTime.fromISO(this.data.scoreHistory[0].recordedAt);
            console.log(`[selectedDate::chart]=>`, this.selectedDate.toISODate());
        }       
        ChartService.drawPieChart(this.data.current, "overall_pie", 300);
        ChartService.drawBarChart(this.data.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked, 100, undefined, 4, this.data.component.id);
    }

    dayClicked(date: string, component: string) {
        console.log(`[] dayClicked ${date}`)
        //this.selectedDate = date;
        router.push('/component-detail/' + encodeURIComponent(component) + '/' + date);
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