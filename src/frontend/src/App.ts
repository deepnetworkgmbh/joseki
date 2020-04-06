import { Component, Vue, Watch } from "vue-property-decorator";
import { InfrastructureComponent } from './models';

@Component
export default class App extends Vue {

  date: string = '';
  component: InfrastructureComponent = new InfrastructureComponent();
  sideWindowOpen: boolean = true;

  /**
   * Method for component change (for breadcrumb).
   *
   * @param {InfrastructureComponent} component
   * @memberof App
   */
  componentChanged(component: InfrastructureComponent) {
    this.component = component;
  }

  /**
   * Method for date change (for breadcrumb).
   *
   * @param {string} date
   * @memberof App
   */
  dateChanged(date: string) {
    this.date = date;
  }

  /**
   * Always scroll to top when navigation occurs.
   *
   * @memberof App
   */
  @Watch('$route.name')
  onRouteChanged() {
    var element = <HTMLElement>document.getElementById("nav");
    var top = element.offsetTop;
    window.scrollTo(0, top);
  }
}
