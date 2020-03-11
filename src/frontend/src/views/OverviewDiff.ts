import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, InfrastructureOverviewDiff, InfrastructureComponent } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';

@Component({
    components: { Spinner, StatusBar }
})
export default class OverviewDiff extends Vue {

    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureOverviewDiff = new InfrastructureOverviewDiff();

    checkedScans: any[] = [];

    created() {
        console.log(`[] ${this.date} vs ${this.date2}`)
        window.addEventListener("resize", this.setupCharts);
        this.loadData();
    }

    loadData() {
        this.service.getGeneralOverviewDiffData(this.date, this.date2)
            .then(response => {
                this.data = response;
                console.log(`[] data is`, this.data);
                this.loaded = true;
                this.setupCharts();
            }).catch(error => {
                console.log(error);
            });
    }

    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        // TODO: ugly fix, getter does not work
        for (let i = 0; i < this.data.components1.length; i++) {
            this.data.components1[i].sections = InfrastructureComponentSummary.getSections(this.data.components1[i].current);
        }
        for (let i = 0; i < this.data.components2.length; i++) {
            this.data.components2[i].sections = InfrastructureComponentSummary.getSections(this.data.components2[i].current);
        }
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }

    goDiffPage(component: InfrastructureComponent) {
        console.log(`[] id: ${component.id} dates: ${this.date} , ${this.date2}`);
        router.push('/component-diff/' + component.id + '/' + this.date + '/' + this.date2);
    }

    drawCharts() {
        ChartService.drawPieChart(this.data.overall1.current, "overall_pie1", 200)
        ChartService.drawPieChart(this.data.overall2.current, "overall_pie2", 200)
        for (let i = 0; i < this.data.components1.length; i++) {
            ChartService.drawBarChart(this.data.components1[i].scoreHistory, 'bar' + i, new Date(this.date), undefined, 48, this.date2);
        }
    }

    getClusters1() { return this.data.components1.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions1() { return this.data.components1.filter(x => x.component.category === 'Subscription').length; }
    getClusters2() { return this.data.components2.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions2() { return this.data.components2.filter(x => x.component.category === 'Subscription').length; }
    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
}