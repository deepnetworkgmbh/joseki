import { Component, Vue, Prop } from "vue-property-decorator";
import AuthService from '@/services/AuthService';
import { ConfigService } from '@/services';
import { Subscription } from 'rxjs';

/**
 * Landing page with login option
 *
 * @export
 * @class HomeComponent
 * @extends {Vue}
 */
@Component
export default class HomeComponent extends Vue {

    loggedSubscription?: Subscription;
    noRoleSubscription?: Subscription;
    hasNoRole: boolean = false;

    created() { 
        if (ConfigService.AuthEnabled) {
            this.noRoleSubscription = AuthService.getInstance().NoRoleAssigned.subscribe((hasNoRole)=>{
                if(hasNoRole) {
                    this.hasNoRole = true;
                }
            })
            this.loggedSubscription = AuthService.getInstance().IsLoggedIn.subscribe((loggedIn)=>{
                if(loggedIn) {
                    this.goToOverview();
                }
            })
        }else{
            this.goToOverview();
        }        
    }

    goToOverview() {
        this.$router.push('overview');
    }

    isAuthenticated() {
        if (ConfigService.AuthEnabled) {
            return this.$msal.isAuthenticated()
        }else{
            return true;
        }
    }


    login() {
        if (!this.$msal.isAuthenticated()) {
            this.$msal.signIn();
        }
    }

    logout() {
        this.$msal.signOut();
    }

    destroyed() {
        if (this.loggedSubscription) {
            this.loggedSubscription.unsubscribe();
        }
    }
}
