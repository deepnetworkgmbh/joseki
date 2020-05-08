import Vue from "vue";
import App from "./App.vue";
import router from "./router";

import "@/styles/main.scss";

import moment from 'moment';
Vue.filter('formatDate', (value) => {
  if (value) { return moment(String(value)).format('YYYY/MM/DD') }
});

import linkify from 'vue-linkify'
Vue.directive('linkified', linkify);

// // @ts-ignore
import VueApexCharts from 'vue-apexcharts'
Vue.use(VueApexCharts);
Vue.component('apexchart', VueApexCharts);


import VueMarkdown from 'vue-markdown-v2';
Vue.component('vue-markdown', VueMarkdown);

import Breadcrumbs from '@/components/breadcrumbs/Breadcrumbs.vue';
Vue.component('Breadcrumbs', Breadcrumbs);
import InfComponent from '@/components/component/InfComponent.vue';
Vue.component('InfComponent', InfComponent);
import DiffComponent from '@/components/component/DiffComponent.vue';
Vue.component('DiffComponent', DiffComponent);
import ControlGroup from '@/components/controlgroup/ControlGroup.vue';
Vue.component('ControlGroup', ControlGroup);
import ControlList from '@/components/controllist/ControlList.vue';
Vue.component('ControlList', ControlList);
import StatusBar from '@/components/statusbar/StatusBar.vue';
Vue.component('StatusBar', StatusBar);
import Score from '@/components/score/Score.vue';
Vue.component('Score', Score);
import ResultFilter from '@/components/filter/ResultFilter.vue';
Vue.component('ResultFilter', ResultFilter);
import AdvancedFilter from '@/components/filter/AdvancedFilter.vue';
Vue.component('AdvancedFilter', AdvancedFilter);
import Navigation from '@/components/navigation/Navigation.vue';
Vue.component('Navigation', Navigation);
import Spinner from '@/components/spinner/Spinner.vue';
Vue.component('Spinner', Spinner);
import Paginator from '@/components/paginator/Paginator.vue';
Vue.component('Paginator', Paginator);

import { MetaService } from './services/MetaService';
import { ConfigService } from './services/ConfigService';

(async () => {

  await ConfigService.Init();
  await MetaService.Init();
  new Vue({ router, render: h => h(App) }).$mount("#app");

})()


