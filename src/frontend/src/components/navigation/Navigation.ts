import { Component, Vue } from "vue-property-decorator";
import { ConfigService } from '@/services';

@Component
export default class Navigation extends Vue {

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
