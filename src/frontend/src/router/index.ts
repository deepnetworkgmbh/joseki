import Vue from "vue";
import VueRouter from "vue-router";
import Overview from "@/views/Overview.vue";
import ImageDetail from "@/views/ImageDetail.vue";
import OverviewDiff from '@/views/OverviewDiff.vue';
import ComponentHistory from '@/views/ComponentHistory.vue';
import ComponentDetail from '@/views/ComponentDetail.vue';
import ComponentDiff from '@/views/ComponentDiff.vue';

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    redirect: "overview"
  },
  {
    path: "/overview/:date?",
    name: "GeneralOverview",
    component: Overview,
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
  }
];

const router: VueRouter = new VueRouter({
  mode: "history",
  routes
});

export default router;
