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
  acceptedRoles = ['JosekiReader', 'JosekiAdmin'];

  created() {
    const clientId = ConfigService.ClientID;
 
    if (ConfigService.AuthEnabled && this.$msal.isAuthenticated()) {
      const roles = this.$msal.data.user["idToken"].roles;
      if (roles === undefined) {
        console.log(`[app] you don't have any Joseki roles assigned`);
        AuthService.getInstance().NoRoleAssigned.next(true);
        return;
      }

      const hasAcceptableRole = this.acceptedRoles.some(r=> roles.indexOf(r) >= 0)
      if (!hasAcceptableRole) {
        AuthService.getInstance().NoRoleAssigned.next(true);
        console.log(`[app] you don't have any Joseki roles assigned`);
        return;
      }

      console.log('[app] user roles', roles);
      AuthService.getInstance().Roles.next(roles)
      console.log('[app] getting access token');
      this.$msal
          .acquireToken({ scopes: [`api://${clientId}/user_impersonation`]})
          .then((result)=> { 
              AuthService.getInstance().AccessToken.next(result.toString());
              AuthService.getInstance().IsLoggedIn.next(true);
              console.log('[app] setting access token', result.toString());
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
