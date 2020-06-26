import Vue from "vue";
import VueRouter from "vue-router";
import Overview from "@/views/Overview.vue";
import OverviewDetail from '@/views/OverviewDetail.vue';
import OverviewDiff from '@/views/OverviewDiff.vue';
import ImageDetail from "@/views/ImageDetail.vue";
import ComponentHistory from '@/views/ComponentHistory.vue';
import ComponentDetail from '@/views/ComponentDetail.vue';
import ComponentDiff from '@/views/ComponentDiff.vue';
import CheckDetail from '@/views/CheckDetail.vue';
import { ConfigService } from '@/services';
import HomeComponent from '@/views/HomeComponent.vue';

Vue.use(VueRouter);

const router: VueRouter = new VueRouter({
  mode: "history",
  routes: [
    {
      path: "/",
      redirect: to => {
        return ConfigService.AuthEnabled ? "home" : "overview"
      }
    },
    {
      path: "/home",
      name: "LandingPage",
      component: HomeComponent,
      props: true
    },
    {
      path: "/overview/:date?",
      name: "GeneralOverview",
      component: Overview,
      props: true
    },
    {
      path: "/overview-detail/:date?/:filter?/:sort?",
      name: "OverviewDetail",
      component: OverviewDetail,
      props: true
    },
    {
      path: "/component-history/:id?",
      name: "ComponentHistory",
      component: ComponentHistory,
      props: true
    },
    {
      path: "/component-detail/:id/:date?",
      name: "ComponentDetail",
      component: ComponentDetail,
      props: true
    },
    {
      path: "/component-diff/:id/:date/:date2",
      name: "ComponentDiff",
      component: ComponentDiff,
      props: true
    },
    {
      path: "/overview-diff/:date/:date2",
      name: "OverviewDiff",
      component: OverviewDiff,
      props: true
    },
    {
      path: "/image-detail/:imageid/:date",
      name: "ImageDetail",
      component: ImageDetail,
      props: true
    },
    {
      path: "/check-detail/:checkid",
      name: "CheckDetail",
      component: CheckDetail,
      props: true
    }
  ]
});

export default router;
