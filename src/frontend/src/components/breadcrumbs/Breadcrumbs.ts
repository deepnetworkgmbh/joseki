import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';
import { InfrastructureComponent } from '@/models';

@Component
export default class Breadcrumbs extends Vue {

    @Prop({default: ''})
    date!: string;

    @Prop({default: undefined })
    component!: InfrastructureComponent

    diffdates: string = ''
    imageid: string = '';
    showDate: boolean = true;
    showComponent: boolean = false;
    showHistory: boolean = false;
    showDiff: boolean = false;
    showImage: boolean = false;

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

    @Watch("date")
    onDateChanged(date: string) {
        console.log(`[brd] date set to ${date}`);
        this.$forceUpdate();
    }

    @Watch("component")
    onComponentChanged(component: InfrastructureComponent) {
        console.log(`[brd] component set to ${component.id}`);
        this.$forceUpdate();
    }

    @Watch("$route.name", { immediate: true})
    onRouteChanged(newPath: string) {        
        this.handleRouteChange();
    }

    handleRouteChange() {
        switch(this.$route.name) {
            case 'GeneralOverview': 
                {
                    let date = this.$route.params.date;
                    console.log(`[r] date: ${date}`);
                    this.showComponent = true;
                    this.showDate = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    break;            
                }
            case 'ComponentDetail': 
                {
                    this.showComponent = true;
                    this.showDate = true;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    break;
                }
            case 'ComponentHistory': 
                {
                    this.showDate = false;
                    this.showComponent = true;
                    this.showHistory = true;
                    this.showDiff = false;
                    this.showImage = false;
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
                    break;
                }
            default:
                {
                    this.showComponent = false;
                    this.showDate = false;
                    this.showHistory = false;
                    this.showDiff = false;
                    this.showImage = false;
                    break;
                }
        }
    }

}