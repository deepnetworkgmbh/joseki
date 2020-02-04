import { Component, Vue } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { KubeOverview } from '@/models';
import { ViewMode } from '@/types/Enums';
import { ScanSummary } from '@/models/ScanSummary';
import { InfrastructureOverview, InfrastructureComponentSummary } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';

@Component({
    components: { Spinner, StatusBar }
})
export default class Overview extends Vue {

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureOverview = new InfrastructureOverview();
    viewMode: ViewMode = ViewMode.detailed;
    grade: string = '?';

    created() {
        this.service.getGeneralOverviewData()
            .then(response => {
                this.data = response;
                console.log(`[] data is`, this.data);
                this.loaded = true;
                this.setupCharts();
            });
        window.addEventListener("resize", this.setupCharts);
 
    }

    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        for(let i=0;i<this.data.components.length;i++) {
            // ugly fix, getter does not work
            this.data.components[i].sections = InfrastructureComponentSummary.getSections(this.data.components[i].current);   
        }
        google.load("visualization", "1", { packages: ["corechart"] });
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }

 
    drawCharts() {
        ChartService.drawPieChart(this.data.overall.current, (this.$refs.chart2 as HTMLInputElement), 300)
        ChartService.drawBarChart(this.data.overall.scoreHistory, (this.$refs.chart3 as HTMLInputElement))
        for(let i=0;i<this.data.components.length;i++) {
            ChartService.drawBarChart(this.data.components[i].scoreHistory, 'bar'+i, 48);
        }
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

        if(score>0 && score <=25) {
            result = "fa fa-poo-storm"            
        }
        if(score>25 && score <=50) {
            result = "fa fa-cloud-rain"            
        }
        if(score>50 && score <=75) {
            result = "fa fa-cloud-sun"            
        }
        if(score>75) {
            result = "fa fa-sun"            
        }

        return result;
    }

    getClusters() { return this.data.components.filter(x=> x.category === 'Kubernetes').length; }
    getSubscriptions() { return this.data.components.filter(x=> x.category === 'Subscription').length; }


    setViewMode(vm: ViewMode) {
        this.viewMode = vm;
    }


}