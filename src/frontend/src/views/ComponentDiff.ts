import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, InfrastructureComponentDiff } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { MappingService } from '@/services/MappingService';

@Component({
    components: { Spinner, StatusBar }
})
export default class ComponentDiff extends Vue {

    @Prop({ default: '' })
    id!: string;
    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentDiff = new InfrastructureComponentDiff();
    checkedScans: any[] = [];

    loadData() {
        this.service
            .getComponentDiffData(this.id, this.date, this.date2)
            .then(response => {
                if(response) {
                    this.data = response;
                    this.setupCharts();
                    this.loaded = true;    
                }
            });
    }

    created() {
        console.log(`[] id: ${this.id} dates: ${this.date} vs ${this.date2}`)
        window.addEventListener("resize", this.setupCharts);
        this.loadData();
    }

    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
        console.log(`[] results diff`, this.ResultsByDiff);
    }


    drawCharts() {
        ChartService.drawPieChart(this.data.summary1.current, "overall_pie1", 200)
        ChartService.drawPieChart(this.data.summary2.current, "overall_pie2", 200)
    }

    dayClicked(date: string) {
        console.log(`[] date clicked ${date}`);
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }

    get ResultsByCollection1() { return MappingService.getResultsByCollection(this.data.summary1.checks) }
    get ResultsByCollection2() { return MappingService.getResultsByCollection(this.data.summary2.checks) }
    get ResultsByDiff() { return MappingService.getResultsDiff(this.data.summary1.checks, this.data.summary2.checks) }
    get scanDetail1url() { return '/component-detail/' + this.data.summary1.component.id + '/' + this.date }
    get scanDetail2url() { return '/component-detail/' + this.data.summary2.component.id + '/' + this.date2 }
}