import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { ChartService } from "@/services/ChartService"
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";
import Score from "@/components/score/Score.vue";
import { DataService } from '@/services/DataService';
import { InfrastructureComponentDiff } from '@/models';
import { ScoreService } from '@/services/ScoreService';
import ControlList from "@/components/controllist/ControlList.vue";
import ControlGroup from "@/components/controlgroup/ControlGroup.vue";
import { CheckObject, DiffCounters, DiffOperation } from '@/services/DiffService';
import { DateTime } from 'luxon';

@Component({
    components: { Spinner, StatusBar, Score, ControlList, ControlGroup }
})
export default class ComponentDiff extends Vue {

    @Prop({ default: '' })
    id!: string;
    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    nochanges: boolean = false;
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentDiff = new InfrastructureComponentDiff();
    checkedScans: any[] = [];

    loadData() {
        this.service
            .getComponentDiffData(this.id, this.date, this.date2)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.nochanges = this.data.results.length === this.data.results.filter(x=>x.operation === DiffOperation.Same).length;
                    this.$emit('componentChanged', this.data.summary1.component);
                    // console.info(JSON.parse(JSON.stringify(this.data.results)));
                    this.loaded = true;
                }
            });
    }

    getDiffAreaChartOptions() {
        return ChartService.DiffAreaChartOptions('diffchart', [this.date, this.date2]);
    }

    getDiffAreaSeries() {
        return [
            {
                name: 'No Data',
                data: [this.data.summary1.current.noData, this.data.summary2.current.noData]
            },
            {
                name: 'Failed',
                data: [this.data.summary1.current.failed, this.data.summary2.current.failed]
            },
            {
                name: 'Warning',
                data: [this.data.summary1.current.warning, this.data.summary2.current.warning]
            },
            {
                name: 'Success',
                data: [this.data.summary1.current.passed, this.data.summary2.current.passed]
            }
        ]
    }

    created() {
        this.loadData();
    }

    getPieChartSeries1() {
        return this.data.summary1.current.getSeries()
    }

    getPieChartSeries2() {
        return this.data.summary2.current.getSeries()
    }

    getPieChartOptions1() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall1", this.data.summary1.current, true)
    }

    getPieChartOptions2() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall2", this.data.summary2.current, true)
    }

    toggleOther(id:string, rowkey: string, objid: string) {
        let element:HTMLInputElement | null = <HTMLInputElement>document.getElementById(id);
        let rowIndex = this.data.results.findIndex(x=>x.key === rowkey);
        if(rowIndex !== -1) {
            let target = id.startsWith('left') ? this.data.results[rowIndex].right : this.data.results[rowIndex].left;
            let other = id.startsWith('left') ? this.data.results[rowIndex].right : this.data.results[rowIndex].left;
            let otherId = id.startsWith('left') ? id.replace('left', 'right') : id.replace('right', 'left')
            if(target) {
                let objIndex = target.objects.findIndex(x=>x.id === objid);
                if(objIndex !== -1) {
                    target.objects[objIndex].checked = element.checked;      
                    setTimeout(toggleHeight, 10, id, otherId, other!.empty, element.checked);
                }
            }    
        }

        function toggleHeight(id, otherId, isOtherEmpty, checked) {
            let thisHeight = document.getElementById(id)!.parentElement!.clientHeight;
            if(isOtherEmpty) {
                // only set the other
                let otherElement = document.getElementById(otherId)!.parentElement;         
                otherElement!.style.height =  thisHeight + 'px';    
            }else{
                let otherHeight = document.getElementById(otherId)!.parentElement!.clientHeight;
                let max = Math.max(thisHeight, otherHeight);
                // set both heights to max or min
                let htmlElement = document.getElementById(id)!.parentElement;         
                htmlElement!.style.height =  checked ? max + 'px' : 'inherit';    
                let otherElement = document.getElementById(otherId)!.parentElement;         
                otherElement!.style.height =   checked ? max + 'px' : 'inherit';    
            }

        }
    }

    sleep(milliseconds) {
        const date = Date.now();
        let currentDate;
        do {
          currentDate = Date.now();
        } while (currentDate - date < milliseconds);
      }

    getScoreIconClass(score: number) { return ScoreService.getScoreIconClass(score); }
    getGrade(score: number) { return ScoreService.getGrade(score); }
    get scanDetail1url() { return '/component-detail/' + this.data.summary1.component.id + '/' + this.date }
    get scanDetail2url() { return '/component-detail/' + this.data.summary2.component.id + '/' + this.date2 }

    getWrapperClass(obj:CheckObject): string {
        if(obj.empty) return `diff-wrapper diff-wrapper-EMPTY`;
         return `diff-wrapper diff-wrapper-${obj.operation}`;
     }
    getRowClass(operation:string): string { return `diff-row diff-row-${operation}`; }
    getObjectClass(obj:CheckObject): string {         
        return `diff-${obj.operation}`; 
    }

    getObjectContainerClass(obj:CheckObject): string {
        return obj.empty ? `diff-object diff-spacer` : `diff-object-${obj.operation}`; 
    }
    getRowTitle(operation:string, changes: DiffCounters): string {
        switch(operation) {
            case 'SAME'     : return 'No changes';
            case 'ADDED'    : return 'Object added';
            case 'REMOVED'  : return 'Object removed';
            case 'CHANGED'  : 
                if(changes.total === 0) {
                    return 'No Changes'
                }else if(changes.total === 1) {
                    return '1 change occured within object'
                }else {
                    return `${changes.total} changes occured within object (${changes.toString()})`
                }
        }
        return ''
    }
}