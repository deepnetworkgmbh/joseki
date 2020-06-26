import { Component, Vue, Prop } from "vue-property-decorator";
import AuthService from '@/services/AuthService';

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
        AuthService.getInstance().IsLoggedIn.subscribe((loggedIn)=>{
            if(loggedIn === 1) {
                this.$router.push('overview/2020-06-19');
            }
        })
    }

    isAuthenticated() {
        return this.$msal.isAuthenticated()
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
