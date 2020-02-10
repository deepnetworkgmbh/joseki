import Vue from "vue";
import VueRouter from "vue-router";
import Overview from "@/views/Overview.vue";
import ClusterOverview from "@/views/ClusterOverview.vue";
import ImageOverview from "@/views/ImageOverview.vue";
import ImageDetail from "@/views/ImageDetail.vue";

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
