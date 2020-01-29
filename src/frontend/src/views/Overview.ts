import { Component, Vue } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { KubeOverview } from '@/models';
import { ViewMode } from '@/types/Enums';
import { ScanSummary } from '@/models/ScanSummary';

@Component({
    components: { Spinner, StatusBar }
})
export default class Overview extends Vue {

    loaded: boolean = false;
    service: DataService = new DataService();
    data: KubeOverview = new KubeOverview();
    viewMode: ViewMode = ViewMode.detailed;
    scans: ScanSummary[] = [];

    created() {
        this.service.getOverviewData()
            .then(response => {
                this.data = response;
                this.loaded = true;
                this.setupCharts();
            });
        window.addEventListener("resize", this.setupCharts);
    }

    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        this.scans = this.service.getDummyGeneralOverview();
        google.load("visualization", "1", { packages: ["corechart"] });
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);

    }

    drawCharts() {
        ChartService.drawPieChart(this.data.checkResultsSummary, (this.$refs.chart2 as HTMLInputElement), 300)
        ChartService.drawAreaChart(this.data.checkResultsSummary, (this.$refs.chart3 as HTMLInputElement))
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
        const score = this.data.cluster.score;

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

        //return "fa fa-poo-storm";
        //return "fa fa-cloud-rain";
        //return "fa fa-cloud-sun";
        //return "fa fa-sun";
        return result;
    }


    setViewMode(vm: ViewMode) {
        this.viewMode = vm;
    }


}