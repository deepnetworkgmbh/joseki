import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import "@/styles/main.scss";

import Donut from 'vue-css-donut-chart';
import 'vue-css-donut-chart/dist/vcdonut.css';
Vue.use(Donut);

import moment from 'moment';

Vue.filter('formatDate', function (value) {
  if (value) { return moment(String(value)).format('YYYY/MM/DD') }
});

import linkify from 'vue-linkify'
import { MetaService } from './services/MetaService';
import { ConfigService } from './services/ConfigService';
Vue.directive('linkified', linkify);

(async()=>{

  await ConfigService.Init();
  await MetaService.Init();
  new Vue({ router, render: h => h(App) }).$mount("#app");

})()
