import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';

import { DataService, ScoreService, MetaService } from '@/services/';
import { InfrastructureComponentSummary, InfrastructureComponent } from '@/models';

/**
 * Component history view lists all the scans occurred for this component.
 * It facilitates the compare function by enabling a couple date input
 *
 * @export
 * @class ComponentHistory
 * @extends {Vue}
 */
@Component
export default class ComponentHistory extends Vue {

    @Prop({ default: '' })
    id!: string;

    component?: InfrastructureComponent;
    loaded: boolean = false;
    service: DataService = new DataService();
    data: InfrastructureComponentSummary[] = [];
    checkedScans: any[] = [];

    /**
     * Loads data when component is created
     *
     * @memberof ComponentHistory
     */
    created() {
        this.loadData();
    }

    /**
     * Makes an api call and gets component history data
     *
     * @memberof ComponentHistory
     */
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

    /**
     * Gets row class for scan list
     *
     * @param {number} i
     * @returns {string}
     * @memberof ComponentHistory
     */
    getScanRowClass(i: number): string {
        return i % 2 === 0 ? 'bg-gray-100' : 'bg-gray-200';
    }

    /**
     * Checks if a comparison can be made
     *
     * @returns {boolean}
     * @memberof ComponentHistory
     */
    canCompare(): boolean {
        return this.checkedScans.length !== 2;
    }

    /**
     * Checks if two dates are already picked,
     * if it is so disables the rest of the checks
     *
     * @param {number} i
     * @param {string} val
     * @returns
     * @memberof ComponentHistory
     */
    checkDisabled(i: number, val: string) {
        return this.checkedScans.length > 1 && this.checkedScans.indexOf(val) === -1
    }

    /**
     * sorts the input dates and redirects to either
     * overview-diff (if this is an overview scan history)
     * or 
     * component-diff (if this is a component scan history)
     *
     * @memberof ComponentHistory
     */
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

    /**
     * returns a link for scan detail
     *
     * @param {string} date
     * @returns
     * @memberof ComponentHistory
     */
    getscanDetailurl(date:string) { 
        if (this.component && this.component.category === 'Overall') {
            return '/overview/' + date.split('T')[0];
        } else {
            return '/component-detail/' + this.component!.id + '/' + date.split('T')[0];
        }
    }

    /**
     * returns grade from score
     *
     * @param {number} score
     * @returns grade
     * @memberof ComponentHistory
     */
    getGrade(score: number) { return ScoreService.getGrade(score); }

    /**
     * returns meta data from MetaService
     *
     * @param {string} key
     * @returns
     * @memberof ComponentHistory
     */
    meta(key: string) { return MetaService.Get(key) }
}