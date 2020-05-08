import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { DataService, ScoreService, MappingService, ChartService } from '@/services/';
import { InfrastructureComponentSummary, ScoreHistoryItem, SeverityFilter, InfrastructureComponent } from '@/models';

import  MarkdownIt  from 'markdown-it';

@Component
export default class CheckDetail extends Vue {
    @Prop()
    checkid!: string;

    @Prop({ default: null })
    date!: string;

    @Prop()
    component?: InfrastructureComponent


    selectedDate?: DateTime;
    loaded: boolean = false;
    loadFailed: boolean = false;
    service: DataService = new DataService();
    documentation: string = '';
    edit: boolean = false;

    md: MarkdownIt;

    created() {
        this.md = new MarkdownIt();
        
        this.loadData();
        this.$emit('dateChanged', DateTime.fromISO(this.date!).toISODate())
        if(this.component) {
            this.$emit('componentChanged', this.component);
        }
    }
    /**
     * make an api call and load Component detail data
     *
     * @memberof CheckDetail
     */
    loadData() {
        this.selectedDate = (this.date === null) ? undefined : DateTime.fromISO(this.date);

        var requestedDocuments = ['checks.' + this.checkid];
        this.service
            .getKnowledgebaseData(requestedDocuments)
            .then(response => {
                if (response) {
                    console.log(response);
                    this.documentation = this.md.render(response[0].content);
                    this.loaded = true;
                    this.$forceUpdate();
                }
            })
            .catch(() => { this.loadFailed = true; });
    }


    /**
     * Watcher for date, emits dateChanged for breadcrumbs and loads data
     *
     * @private
     * @param {string} newValue
     * @memberof CheckDetail
     */
    @Watch('id', { immediate: true })
    private onDateChanged(newValue: string) {
        this.selectedDate = DateTime.fromISO(newValue);
        this.$emit('dateChanged', this.selectedDate.toISODate())
        this.loadData();
    }
}
