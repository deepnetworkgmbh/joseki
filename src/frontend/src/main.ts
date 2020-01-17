import Vue from "vue";
import App from "./App.vue";
import router from "./router";

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

import "@/styles/main.scss";

new Vue({
  router,
  render: h => h(App)
}).$mount("#app");
