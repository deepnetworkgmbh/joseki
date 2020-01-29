import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import "@/styles/main.scss";

// include font awesome
import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faShieldAlt,
  faTimes,
  faQuestionCircle
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

library.add(faShieldAlt, faTimes, faQuestionCircle);
Vue.component("font-awesome-icon", FontAwesomeIcon);
Vue.config.productionTip = false;

import Donut from 'vue-css-donut-chart';
import 'vue-css-donut-chart/dist/vcdonut.css';
Vue.use(Donut);

new Vue({
  router,
  render: h => h(App)
}).$mount("#app");
