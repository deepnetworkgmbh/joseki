import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { InfrastructureComponent } from '@/models';
import router from '@/router';

@Component
export default class Breadcrumbs extends Vue {

    @Prop({default: ''})
    date!: string;

    @Prop({default: undefined })
    component!: InfrastructureComponent

    diffdates: string = ''
    imageid: string = '';
    checkid: string = '';
    filter: string = '';

    showDate: boolean = true;
    showComponent: boolean = false;
    showHistory: boolean = false;
    showDiff: boolean = false;
    showImage: boolean = false;
    showFilter: boolean = false;
    showCheck: boolean = false;

    getIcon(): string {
        if(this.component) {
            if(this.component.category === 'Azure Subscription' || this.component.category === 'Subscription') return 'icon-azuredevops';
            if(this.component.category === 'Kubernetes') return 'icon-kubernetes';
        }
        return '';
    }

    getComponentLink(): string {
        if(this.component) {
            if(this.component.category === 'Overall') {
                return '/overview/' + this.date;
            } else {
                return '/component-detail/'+ this.component.id + '/' + this.date;
            }
        }
        return ''
    }

    getComponentLabel(): string {
        if(this.component) {
            if(this.component.category === 'Overall') {
                return this.component.name;
            }else {
                return this.component.category + ' : ' + this.component.name;
            }
        }
        return ''
    }

    onFilterChangedFromAF(filter: string) {
        this.filter = filter;
        this.$router.push({ name: router.currentRoute!.name!.toString(), params: { date: this.date, filter: filter }})
    }

    @Watch("date")
    onDateChanged(date: string) {
        this.$forceUpdate();
    }

    @Watch("component")
    onComponentChanged(component: InfrastructureComponent) {
        this.$forceUpdate();
    }

    @Watch("$route.name", { immediate: true})
    onRouteChanged(newPath: string) {        
        this.handleRouteChange();
    }

    @Watch("$route.params", { immediate: true})
    onRouteParamsChanged(newPath: string) {        
        this.handleRouteChange();
    }

    @Watch("filter", { immediate: true})
    onFilterChanged(newfilter: string) {                
        this.handleRouteChange();
    }

    handleRouteChange() {
        switch(this.$route.name) {
            case 'GeneralOverview': 
                {
                    let date = this.$route.params.date;
                    this.showComponent = true;
                    this.showDate = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;            
                }
            case 'ComponentDetail': 
                {
                    this.showComponent = true;
                    this.showDate = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
            case 'ComponentHistory': 
                {
                    this.showDate = false;
                    this.showComponent = true;
                    this.showHistory = true;
                    this.showDiff = false;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
            case 'ComponentDiff': 
                {
                    this.showDate = false;
                    this.showComponent = true;
                    this.showHistory = false;
                    this.showDiff = true;
                    this.diffdates = this.$route.params.date + ' / ' + this.$route.params.date2;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
            case 'OverviewDiff': 
                {
                    this.showDate = false;
                    this.showComponent = true;
                    this.showHistory = false;
                    this.showDiff = true;
                    this.diffdates = this.$route.params.date + ' / ' + this.$route.params.date2;
                    this.showImage = false;                 
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
            case 'OverviewDetail': 
                {
                    this.showDate = true;
                    this.showComponent = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.diffdates = '';
                    this.showImage = false;                 
                    this.showFilter = this.$route.params.filter !== undefined;
                    this.filter = this.$route.params.filter;                    
                    this.showCheck = false;
                    break;
                }
            case 'ImageDetail':
                {
                    this.showDate = true;
                    this.showComponent = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = true;
                    this.imageid = this.$route.params.imageid;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
            case 'CheckDetail':
                {
                    this.showDate = true;
                    this.showComponent = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = true;
                    this.checkid = this.$route.params.checkid;
                    break;
                }
            default:
                {
                    this.showComponent = false;
                    this.showDate = false;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    this.showFilter = false;
                    this.showCheck = false;
                    break;
                }
        }
    }

}