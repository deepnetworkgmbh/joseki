import Vue from "vue";
import VueRouter from "vue-router";
import ClusterOverview from "@/views/ClusterOverview.vue";
import ImageOverview from "@/views/ImageOverview.vue";
import ImageDetail from "@/views/ImageDetail.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    redirect: "cluster-overview"
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

const router = new VueRouter({
  mode: "history",
  //base: process.env.BASE_URL,
  routes
});

export default router;
