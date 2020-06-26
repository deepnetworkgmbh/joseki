import { Component, Vue, Watch } from "vue-property-decorator";
import { InfrastructureComponent } from './models';
import { ConfigService } from './services';
import AuthService from './services/AuthService';

@Component
export default class App extends Vue {

  landing: boolean = true;
  date: string = '';
  component: InfrastructureComponent = new InfrastructureComponent();
  wide: boolean = false;
  // list of views that use full-width
  wideControls = ['OverviewDetail'];

  created() {
    const clientId = ConfigService.ClientID;
    if (ConfigService.AuthEnabled && this.$msal.isAuthenticated()) {
      console.log('[app] getting access token');
      this.$msal
          .acquireToken({ scopes: [`api://${clientId}/user_impersonation`]})
          .then((result)=> { 
              AuthService.getInstance().AccessToken.next(result.toString());
              AuthService.getInstance().IsLoggedIn.next(1);
              console.log('[app] setting access token');
          });
    }
  }



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
    this.landing = (routename === 'LandingPage');
    this.wide = this.wideControls.indexOf(routename) !== -1;
    let element = <HTMLElement>document.getElementById("nav");
    if(element) {
      let top = element.offsetTop;
      window.scrollTo(0, top);  
    }
  }
}
