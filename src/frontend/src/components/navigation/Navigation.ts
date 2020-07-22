import { Component, Vue } from "vue-property-decorator";
import { ConfigService } from '@/services';
import AuthService, { User } from '@/services/AuthService';
import { Subscription } from 'rxjs';

@Component
export default class Navigation extends Vue {

    user: User = new User();
    roles: string[] = [];
    userSubscription!: Subscription;
    roleSubscription!: Subscription;

    created() {
        if(this.authEnabled()) {
            this.userSubscription = AuthService.getInstance().User.subscribe((user)=> {
                this.user = user;
            });    
            this.roleSubscription = AuthService.getInstance().Roles.subscribe((roles)=> {
                this.roles = roles;
            });    
        }
    }

    get IsAdmin() {
        return this.roles.indexOf('JosekiAdmin') !== -1;
    }

    destroyed() {
        if(this.authEnabled() && this.userSubscription) {
            this.userSubscription.unsubscribe();
        }
    }

    authEnabled() {
        return ConfigService.AuthEnabled;
    }

    signout() {
        this.$msal.signOut();
    }

    returnClusterOverviewActive(){
        if(this.$route.path === '/cluster-overview') {
         return 'router-link-exact-active';
        }
        return '';
    }

    returnImageOverviewActive(){
        if(this.$route.path === '/image-overview' ||  this.$route.path.startsWith('/image-detail')) {
         return 'router-link-exact-active';
        }
        return '';
    }
}
