import { Component, Vue, Prop } from "vue-property-decorator";
import AuthService from '@/services/AuthService';
import { ConfigService } from '@/services';

/**
 * Landing page with login option
 *
 * @export
 * @class HomeComponent
 * @extends {Vue}
 */
@Component
export default class HomeComponent extends Vue {

    created() { 
        if (ConfigService.AuthEnabled) {
            AuthService.getInstance().IsLoggedIn.subscribe((loggedIn)=>{
                if(loggedIn === 1) {
                    this.goHome();
                }
            })
        }else{
            this.goHome();
        }        
    }

    goHome() {
        this.$router.push('overview/2020-06-19');
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
}
