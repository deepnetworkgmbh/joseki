
import VueRouter, { Route } from 'vue-router';
import { MSALBasic } from 'vue-msal/lib/src/types';

declare module 'vue/types/vue' {
  interface Vue {
    $router: VueRouter
    $route: Route
    $msal: MSALBasic
  }
}