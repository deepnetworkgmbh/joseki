import { Component, Vue, Prop } from "vue-property-decorator";

import { DataService, ScoreService, ChartService, CheckObject, DiffCounters, DiffOperation } from '@/services/';
import { InfrastructureComponentDiff } from '@/models';

/**
 * ComponentDiff is a view for comparing a component's scans
 * over two different dates
 *
 * @export
 * @class ComponentDiff
 * @extends {Vue}
 */
@Component
export default class ComponentDiff extends Vue {

    @Prop({ default: '' })
    id!: string;
    @Prop({ default: null })
    date!: string;
    @Prop({ default: null })
    date2!: string;

    nochanges: boolean = false;
    loaded: boolean = false;
    loadFailed: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentDiff = new InfrastructureComponentDiff();
    checkedScans: any[] = [];

    /**
     * On component created, load diff data
     *
     * @memberof ComponentDiff
     */
    created() {
        this.loadData();
    }

    /**
     * Makes an api call and gets component diff data
     *
     * @memberof ComponentDiff
     */
    loadData() {
        this.service
            .getComponentDiffData(this.id, this.date, this.date2)
            .then(response => {
                if (response) {
                    this.data = response;
                    this.nochanges = this.data.results.length === this.data.results.filter(x=>x.operation === DiffOperation.Same).length;
                    this.$emit('componentChanged', this.data.summary1.component);
                    this.loaded = true;
                }
            })
            .catch(()=> { this.loadFailed = true; });
    }


    /**
     * Series data for pie chart 1
     *
     * @returns
     * @memberof ComponentDiff
     */
    getPieChartSeries1() {
        return this.data.summary1.current.getSeries()
    }

    /**
     * Series data for pie chart 2
     *
     * @returns
     * @memberof ComponentDiff
     */
    getPieChartSeries2() {
        return this.data.summary2.current.getSeries()
    }

    /**
     * Options for pie chart 1
     *
     * @returns
     * @memberof ComponentDiff
     */
    getPieChartOptions1() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall1", this.data.summary1.current, ()=>{}, true)
    }

    /**
     * Options for pie chart 2
     *
     * @returns
     * @memberof ComponentDiff
     */
    getPieChartOptions2() : ApexCharts.ApexOptions {
        return ChartService.PieChartOptions("pie-overall2", this.data.summary2.current, ()=>{},true)
    }

    /**
     * toggles the other side of the selection
     * and sets the height equal to the current selection
     *
     * @param {string} id
     * @param {string} rowkey
     * @param {string} objid
     * @memberof ComponentDiff
     */
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

    /**
     * a delaying utility function
     *
     * @param {*} milliseconds
     * @memberof ComponentDiff
     */
    sleep(milliseconds) {
        const date = Date.now();
        let currentDate;
        do {
          currentDate = Date.now();
        } while (currentDate - date < milliseconds);
    }

    /**
     * returns url for scan1 detail
     *
     * @readonly
     * @memberof ComponentDiff
     */
    get scanDetail1url() { return '/component-detail/' + this.data.summary1.component.id + '/' + this.date }

    /**
     * returns url for scan2 detail
     *
     * @readonly
     * @memberof ComponentDiff
     */
    get scanDetail2url() { return '/component-detail/' + this.data.summary2.component.id + '/' + this.date2 }

    /**
     * returns grade froms score
     *
     * @readonly
     * @memberof ComponentDiff
     */
    getGrade(score: number) { return ScoreService.getGrade(score); }

    /**
     * returns wrapper class for CheckObject
     *
     * @param {CheckObject} obj
     * @returns {string}
     * @memberof ComponentDiff
     */
    getWrapperClass(obj:CheckObject): string {
        if(obj.empty) return `diff-wrapper diff-wrapper-EMPTY`;
        return `diff-wrapper diff-wrapper-${obj.operation}`;
    }

    /**
     * returns row class using DiffOperation
     *
     * @param {string} operation
     * @returns {string}
     * @memberof ComponentDiff
     */
    getRowClass(operation:string): string { 
        return `diff-row diff-row-${operation}`; 
    }

    /**
     * get class for CheckObject
     *
     * @param {CheckObject} obj
     * @returns {string}
     * @memberof ComponentDiff
     */
    getObjectContainerClass(obj:CheckObject): string {
        return obj.empty ? `diff-object diff-spacer` : `diff-object-${obj.operation}`; 
    }

    /**
     * get row title using DiffOperation/DiffCounters
     *
     * @param {string} operation
     * @param {DiffCounters} changes
     * @returns {string}
     * @memberof ComponentDiff
     */
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