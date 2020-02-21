import Vue from "vue";
import VueRouter from "vue-router";
import Overview from "@/views/Overview.vue";
import ClusterOverview from "@/views/ClusterOverview.vue";
import ImageOverview from "@/views/ImageOverview.vue";
import ImageDetail from "@/views/ImageDetail.vue";
import OverviewDiff from '@/views/OverviewDiff.vue';
import ComponentHistory from '@/views/ComponentHistory.vue';
import ComponentDetail from '@/views/ComponentDetail.vue';

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
    path: "/overview-diff/:date/:date2",
    name: "OverviewDiff",
    component: OverviewDiff,
    props: true
  },
  {
    path: "/cluster-overview",
    name: "ClusterOverview",
    component: ClusterOverview
  },
  {
    path: "/image-overview",
    name: "ImageOverview",
    component: ImageOverview
  },
  {
    path: "/image-detail/:imageid",
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
