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
        this.loadData();
    }

    loadData() {
        this.service.getGeneralOverviewDiffData(this.date, this.date2)
            .then(response => {
                if(response) {
                    this.data = response;
                    this.loaded = true;
                }
            }).catch(error => console.log(error));
    }

    getDiffAreaChartOptions() {
        return ChartService.DiffAreaChartOptions('diffchart', [this.date, this.date2]);
    }

    getDiffAreaSeries() {
        return [
            {
                name: 'No Data',
                data: [this.data.summary1.overall.current.noData, this.data.summary2.overall.current.noData]
            },
            {
                name: 'Failed',
                data: [this.data.summary1.overall.current.failed, this.data.summary2.overall.current.failed]
            },
            {
                name: 'Warning',
                data: [this.data.summary1.overall.current.warning, this.data.summary2.overall.current.warning]
            },
            {
                name: 'Success',
                data: [this.data.summary1.overall.current.passed, this.data.summary2.overall.current.passed]
            }
        ]
    }

    goDiffPage(component: InfrastructureComponent) {
        router.push('/component-diff/' + component.id + '/' + this.date + '/' + this.date2);
    }

    getClusters1() { return this.data.summary1.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions1() { return this.data.summary1.components.filter(x => x.component.category === 'Azure Subscription').length; }
    getClusters2() { return this.data.summary2.components.filter(x => x.component.category === 'Kubernetes').length; }
    getSubscriptions2() { return this.data.summary2.components.filter(x => x.component.category === 'Azure Subscription').length; }
    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
}