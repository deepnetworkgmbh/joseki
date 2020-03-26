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
                    console.info(JSON.parse(JSON.stringify(this.data.results)));
                    this.setupCharts();
                    this.loaded = true;
                }
            });
    }

    created() {
        console.log(`[] id: ${this.id} dates: ${this.date} vs ${this.date2}`)
        window.addEventListener("resize", this.setupCharts);
        this.loadData();
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
            //console.log(`[]`, id, otherId);
            let thisHeight = document.getElementById(id)!.parentElement!.clientHeight;
            //console.log(`[] this height ${thisHeight}`)
            if(isOtherEmpty) {
                // only set the other
                let otherElement = document.getElementById(otherId)!.parentElement;         
                otherElement!.style.height =  thisHeight + 'px';    
            }else{
                let otherHeight = document.getElementById(otherId)!.parentElement!.clientHeight;
                //console.log(`[] otherHeight ${otherHeight}`)
                let max = Math.max(thisHeight, otherHeight);
                //console.log(`[] max height is ${max}`)
                // set both heights to max or min
                let htmlElement = document.getElementById(id)!.parentElement;         
                htmlElement!.style.height =  checked ? max + 'px' : 'inherit';    
                let otherElement = document.getElementById(otherId)!.parentElement;         
                otherElement!.style.height =   checked ? max + 'px' : 'inherit';    
            }

        }
    }


    destroyed() {
        window.removeEventListener("resize", this.setupCharts);
    }

    setupCharts() {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawCharts);
        //console.log(`[] results diff`, this.data.results);
    }

    sleep(milliseconds) {
        const date = Date.now();
        let currentDate;
        do {
          currentDate = Date.now();
        } while (currentDate - date < milliseconds);
      }

    drawCharts() {
        ChartService.drawPieChart(this.data.summary1.current, "overall_pie1", 200)
        ChartService.drawPieChart(this.data.summary2.current, "overall_pie2", 200)
    }

    dayClicked(date: string) {
        console.log(`[] date clicked ${date}`);
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