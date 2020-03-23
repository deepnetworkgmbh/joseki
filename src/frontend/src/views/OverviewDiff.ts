import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, InfrastructureOverviewDiff, InfrastructureComponent } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import DiffComponent from '@/components/component/DiffComponent.vue';
import router from '@/router';
import { DateTime } from 'luxon';

@Component({
    components: { Spinner, StatusBar, DiffComponent }
})
export default class OverviewDiff extends Vue {

    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    loaded: boolean = false;
    service: DataService = new DataService();
    data!: InfrastructureOverviewDiff;
    checkedScans: any[] = [];

    created() {
        console.log(`[] ${this.date} vs ${this.date2}`)
        window.addEventListener("resize", this.setupCharts);
        this.loadData();
    }

    loadData() {
        this.service.getGeneralOverviewDiffData(this.date, this.date2)
            .then(response => {
                if(response) {
                    this.data = response;
                    console.log(`[] data is`, this.data);
                    this.loaded = true;
                    this.setupCharts();    
                }
            }).catch(error => {
                console.log(error);
            });
    }

    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }

    goDiffPage(component: InfrastructureComponent) {
        console.log(`[] id: ${component.id} dates: ${this.date} , ${this.date2}`);
        router.push('/component-diff/' + component.id + '/' + this.date + '/' + this.date2);
    }

    drawCharts() {
        ChartService.drawPieChart(this.data.summary1.overall.current, "overall_pie1", 200)
        ChartService.drawPieChart(this.data.summary2.overall.current, "overall_pie2", 200)
        let date = DateTime.fromISO(this.date);
        let date2 = DateTime.fromISO(this.date2);
        for (let i = 0; i < this.data.summary1.components.length; i++) {
            if(this.data.summary2.components[i]) {
                ChartService.drawBarChart(this.data.summary1.components[i].scoreHistory, 'bar' + i, date, undefined, 48, date2);
            }
        }
    }

    getClusters1() { return this.data.summary1.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions1() { return this.data.summary1.components.filter(x => x.component.category === 'Subscription').length; }
    getClusters2() { return this.data.summary2.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions2() { return this.data.summary2.components.filter(x => x.component.category === 'Subscription').length; }
    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
}