import { Component, Vue, Watch } from "vue-property-decorator";
import Navigation from "@/components/navigation/Navigation.vue";
import Breadcrumbs from '@/components/breadcrumbs/Breadcrumbs.vue';
import { InfrastructureComponent } from './models';

@Component({
  components: {
    Navigation, Breadcrumbs
  }
})
export default class App extends Vue {

  date: string = '';
  component: InfrastructureComponent = new InfrastructureComponent();

  sideWindowOpen: boolean = true;

  componentChanged(component: InfrastructureComponent) {
    console.log(`[P] component changed: ${component.id}`);
    this.component = component;
  }

  dateChanged(date: string) {
    console.log(`[P] date changed: ${date}`);
    this.date = date;
  }

  getWrapperClass() {
    const bareClass = 'container mx-auto '
    return bareClass + (this.sideWindowOpen ? 'wrapper-shring' : 'wrapper');
  }

  // always scroll to top when navigation occurs
  @Watch('$route.name')
  onRouteChanged() {
    var element = <HTMLElement>document.getElementById("nav");
    var top = element.offsetTop;
    window.scrollTo(0, top);
  }
}
