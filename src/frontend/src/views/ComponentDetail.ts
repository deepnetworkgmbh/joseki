import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponent, CountersSummary, CheckSeverity } from '@/models/InfrastructureOverview';
import { ScoreService } from '@/services/ScoreService';
import router from '@/router';

@Component({
    components: { Spinner, StatusBar }
})
export default class ComponentDetail extends Vue {

    @Prop()
    id!: string;

    @Prop({ default: null })
    date!: string;

    selectedDate: Date = new Date();

    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary = new InfrastructureComponentSummary();
    panelOpen: boolean = false;
    checkedScans: any[] = [];


    created() {
        window.addEventListener("resize", this.setupCharts);
    }

    loadData() {
        let dateString = (this.date === null) ? '' : this.date;
        this.service.getComponentDetailData(this.id, dateString)
            .then(response => {
                this.data = response;
                console.log(`[] data is`, this.data);
                this.data.scoreHistory = this.data.scoreHistory.reverse();
                this.loaded = true;
                this.setupCharts();
            });
    }


    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
    }


    drawCharts() {
        this.selectedDate = this.date ?
            new Date(decodeURIComponent(this.date))
            : this.data.scoreHistory[0].recordedAt;
        ChartService.drawPieChart(this.data.current, "overall_pie", 300)
        ChartService.drawBarChart(this.data.scoreHistory, "overall_bar", this.selectedDate, this.dayClicked)
    }

    dayClicked(date: Date) {
        //console.log(`[] clicked ${date.toISOString()}`)
        router.push('/overview/' + encodeURIComponent(date.toISOString()));
    }

    goComponentHistory() {
        if (this.data.component) {
            router.push('/component-history/' + this.data.component.id);
        } else {
            router.push('/component-history/');
        }
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

    getScanRowClass(i: number): string {
        return i % 2 === 0 ? 'bg-gray-100' : 'bg-gray-200';
    }

    get shortHistory() {
        return this.data.scoreHistory.slice(0, 5);
    }

    onHistoryClicked() {
        router.push('/overview-history/');
    }

    getPanelClass() {
        this.$emit(this.panelOpen ? 'sideWindowOpened' : 'sideWindowClosed');
        return this.panelOpen ? 'right-menu-open' : 'right-menu';
    }

    canCompare(): boolean {
        return this.checkedScans.length !== 2;
    }

    checkDisabled(i: number, val: string) {
        return this.checkedScans.length > 1 && this.checkedScans.indexOf(val) === -1
    }

    CompareScans() {
        console.log(`[] comparing ${this.checkedScans}`);
        const params = encodeURIComponent(this.checkedScans[1]) + '/' + encodeURIComponent(this.checkedScans[0]);
        router.push('/overview-diff/' + params);
    }

    @Watch('id', { immediate: true })
    private onDateChanged(newValue: Date) {
        this.loadData();
    }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }


    get ResultsByCategory() {

        var categoryCounters = {};

        // walk over all checks and group them by cateory.
        for (let i = 0; i < this.data.checks.length; i++) {

            const check = this.data.checks[i];

            if (categoryCounters[check.category] === undefined) {
                categoryCounters[check.category] = new CountersSummary();
            }

            switch (check.result.toString()) {
                case 'Failed':
                    categoryCounters[check.category].failed += 1;
                    break;
                case 'NoData':
                    categoryCounters[check.category].noData += 1;
                    break;
                case 'Warning':
                    categoryCounters[check.category].warning += 1;
                    break;
                case 'Success':
                    categoryCounters[check.category].passed += 1;
                    break;
            }

            categoryCounters[check.category].total += 1;
        }
        //console.log(categoryCounters);

        let keys = Object.keys(categoryCounters);
        for (let i = 0; i < keys.length; i++) {
            categoryCounters[keys[i]].score = categoryCounters[keys[i]].calculateScore();
        }

        return categoryCounters;
    }

    get ResultsByCollection() {

        // collection1
        //  - object1
        //    - control 1
        //    - control 2
        //  - object2
        //    - control 1
        //    - control 2
        // collection2
        //  - object1
        //    - control 1
        //    - control 2
        //  - object2
        //    - control 1
        //    - control 2

        var results = {};

        // walk over all checks and group them by collections.
        for (let i = 0; i < this.data.checks.length; i++) {
            let row = this.data.checks[i];

            if (results[row.collection.name] === undefined) {
                results[row.collection.name] = {
                    type: row.collection.type,
                    name: row.collection.name,
                    counters: new CountersSummary(),
                    score: 0,
                    objects: {},
                };
            }

            switch (row.result.toString()) {
                case 'Failed':
                    results[row.collection.name].counters.failed += 1;
                    break;
                case 'NoData':
                    results[row.collection.name].counters.noData += 1;
                    break;
                case 'Warning':
                    results[row.collection.name].counters.warning += 1;
                    break;
                case 'Success':
                    results[row.collection.name].counters.passed += 1;
                    break;
            }
            results[row.collection.name].counters.total += 1;

            if (results[row.collection.name].objects[row.resource.id] === undefined) {
                results[row.collection.name].objects[row.resource.id] = {
                    type: row.resource.type,
                    name: row.resource.name,
                    controls: []
                }
            }

            results[row.collection.name].objects[row.resource.id].controls.push({
                id: row.control.id,
                text: row.control.message,
                result: row.result,
                icon: this.getControlIcon(row.result),
                order: this.getSeverityScore(row.result)
            });
        }

        let keys = Object.keys(results);
        for (let i = 0; i < keys.length; i++) {
            results[keys[i]].score = results[keys[i]].counters.calculateScore();

            for (let j = 0; j < Object.keys(results[keys[i]].objects).length; j++) {
                let key = Object.keys(results[keys[i]].objects)[j];
                results[keys[i]].objects[key].controls.sort((a, b) => (a.order < b.order) ? -1 : (a.order > b.order) ? 1 : 0)
            }
        }

        console.log(results);
        return results;
    }

    getControlIcon(severity: CheckSeverity) {
        switch (severity.toString()) {
            case 'NoData': return "fas fa-times nodata-icon";
            case 'Failed':
            case 'Warning': return "fas fa-exclamation-triangle warning-icon";
            case 'Success': return "fas fa-check noissues-icon";
        }
    }

    getSeverityScore(severity: CheckSeverity) {
        switch (severity.toString()) {
            case 'NoData': return 100;
            case 'Failed':
            case 'Warning': return 10;
            case 'Success': return 1;
        }
    }

}