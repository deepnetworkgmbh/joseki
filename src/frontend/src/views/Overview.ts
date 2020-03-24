import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import InfComponent from '@/components/component/InfComponent.vue';
import { InfrastructureOverview, InfrastructureComponent } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { DateTime } from 'luxon';

@Component({
    components: { Spinner, StatusBar, InfComponent }
})
export default class Overview extends Vue {

    @Prop({ default: null })
    date!: string;

    selectedDate?: DateTime = undefined;
    loaded: boolean = false;
    service: DataService = new DataService();
    data!: InfrastructureOverview;

    loadData() {
        this.selectedDate = (this.date === null) ? undefined : DateTime.fromISO(this.date);
        if (this.selectedDate !== undefined) {
            console.log(`[selectedDate]=>`, this.selectedDate.toISODate());
        }
        this.service
            .getGeneralOverviewData(this.selectedDate)
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
        this.loadData();
    }

    destroyed() { window.removeEventListener("resize", this.setupCharts); }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }

    drawCharts() {
        if(this.selectedDate === undefined) {
            this.selectedDate = DateTime.fromISO(this.data.overall.scoreHistory[0].recordedAt);
            console.log(`[selectedDate::chart]=>`, this.selectedDate.toISODate());
        }       

        ChartService.drawPieChart(this.data.overall.current, "overall_pie", 300)
        ChartService.drawBarChart(this.data.overall.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked, 100, undefined, 4)
        for (let i = 0; i < this.data.components.length; i++) {
            ChartService.drawBarChart(this.data.components[i].scoreHistory, 'bar' + i, this.selectedDate, this.goComponentDetail, 52, undefined, 0, this.data.components[i].component.id);
        }
        this.$forceUpdate();
    }

    dayClicked(date: string) {
        this.selectedDate = DateTime.fromISO(date);
        console.log(`[] day clicked ${date}`)
        router.push('/overview/' + date);
    }

    goComponentDetail(date: string, componentId: string) {
        router.push('/component-detail/' + componentId + '/' + date);
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

    get shortHistory() {
        return this.data.overall.scoreHistory.slice(0, 5);
    }

    getClusters() { return this.data.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions() { return this.data.components.filter(x => x.component.category === 'Azure Subscription').length; }


    onHistoryClicked() {
        router.push('/overview-history/');
    }

    @Watch('date', { immediate: true })
    private onDateChanged(newValue: string) {
        this.selectedDate = DateTime.fromISO(newValue);
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }

}