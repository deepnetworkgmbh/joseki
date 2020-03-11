import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import Spinner from "@/components/spinner/Spinner.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentSummary, InfrastructureComponent } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';

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
        console.log(`[] calling history for component ${this.id}`)
        this.service
            .getComponentHistoryData(this.id)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.component = response[0].component;                    
                    this.loaded = true;
                }
            });
    }

    destroyed() {
        //window.removeEventListener("resize", this.setupCharts);
    }

    dayClicked(date: Date) {
        router.push('/overview/' + encodeURIComponent(date.toDateString()));
    }

    getArrowHtml(i: number) {
        if (i >= (this.data.length - 1)) return '-';
        if (this.data[i].current.score > this.data[i + 1].current.score) {
            return '<i class="fas fa-arrow-up" style="color:green;"></i>'
        } else if (this.data[i].current.score < this.data[i + 1].current.score) {
            return '<i class="fas fa-arrow-down" style="color:red;"></i>'
        }
        return '-'
    }

    getErrorArrowHtml(key: string, i: number, reverseColor: boolean = false) {
        let data = { class: '-', color: '-' };

        if (i < (this.data.length - 1)) {
            if (this.data[i].current[key] > this.data[i + 1].current[key]) {
                data = { class: 'up', color: reverseColor ? 'green' : 'red' };
            } else if (this.data[i].current[key] < this.data[i + 1].current[key]) {
                data = { class: 'down', color: reverseColor ? 'red' : 'green' };
            }
        }
        if (data.class === '-') return '-'
        return `<i class="fas fa-arrow-${data.class}" style="color:${data.color};"></i>`
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
        if (this.component && this.component.category === 'Overall') {
            console.log(`[] comparing ${this.checkedScans}`);
            const params = encodeURIComponent(this.checkedScans[1]) + '/' + encodeURIComponent(this.checkedScans[0]);
            router.push('/overview-diff/' + params);
        } else {
            if (this.component) {
                const params = this.component.id + '/' + this.checkedScans[1] + '/' + this.checkedScans[0];
                router.push('/component-diff/' + params);
            }
        }
    }

    GoBack() {
        router.go(-1);
        //router.push('/overview/');
    }
    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }

}