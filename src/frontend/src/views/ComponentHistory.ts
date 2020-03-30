import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import Spinner from "@/components/spinner/Spinner.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, InfrastructureComponent, MetaData } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';
import { MetaService } from '@/services/MetaService';

@Component({
    components: { Spinner }
})
export default class ComponentHistory extends Vue {

    @Prop({ default: '' })
    id!: string;

    component?: InfrastructureComponent;
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary[] = [];
    checkedScans: any[] = [];
    
    mounted() {
        this.loadData();
        ///window.addEventListener("resize", this.setupCharts);
    }

    loadData() {
        this.service
            .getComponentHistoryData(this.id)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.component = response[0].component;                    
                    this.$emit('componentChanged', this.component);
                    this.loaded = true;
                }
            });
    }

    destroyed() {
        //window.removeEventListener("resize", this.setupCharts);
    }

    getScanRowClass(i: number): string {
        return i % 2 === 0 ? 'bg-gray-100' : 'bg-gray-200';
    }

    canCompare(): boolean {
        return this.checkedScans.length !== 2;
    }

    checkDisabled(i: number, val: string) {
        return this.checkedScans.length > 1 && this.checkedScans.indexOf(val) === -1
    }

    CompareScans() {
        this.checkedScans.sort();
        let params = this.checkedScans[0].split('T')[0] + '/' + this.checkedScans[1].split('T')[0];
        if (this.component && this.component.category === 'Overall') {
            router.push('/overview-diff/' + params);
        } else {
            if (this.component) {
                params = this.component.id + '/' + params;
                router.push('/component-diff/' + params);
            }
        }
    }

    getscanDetailurl(date:string) { 
        if (this.component && this.component.category === 'Overall') {
            return '/overview/' + date.split('T')[0];
        } else {
            return '/component-detail/' + this.component!.id + '/' + date.split('T')[0];
        }
    }

    GoBack() {
        router.go(-1);
        //router.push('/overview/');
    }
    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
    meta(key: string) { return MetaService.Get(key) }
}