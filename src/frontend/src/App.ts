import { Component, Vue, Watch } from "vue-property-decorator";
import { InfrastructureComponent } from './models';

@Component
export default class App extends Vue {

  date: string = '';
  component: InfrastructureComponent = new InfrastructureComponent();
  wide: boolean = false;
  // list of views that use full-width
  wideControls = ['OverviewDetail'];

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
   * Return full width class if current route is in wideControls list.
   *
   * @returns
   * @memberof App
   */
  getWrapperClass() {
    return this.wide ? 'wrapper-full' : 'wrapper';
  }

  /**
   * Always scroll to top when navigation occurs.
   *
   * @memberof App
   */
  @Watch('$route.name', { immediate: true })
  onRouteChanged(routename: string) {
    this.wide = this.wideControls.indexOf(routename) !== -1;
    var element = <HTMLElement>document.getElementById("nav");
    if(element) {
      var top = element.offsetTop;
      window.scrollTo(0, top);  
    }
  }
}
