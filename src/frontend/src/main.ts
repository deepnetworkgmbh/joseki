import Vue from "vue";
import App from "./App.vue";
import router from "./router";

// include font awesome
import { library } from "@fortawesome/fontawesome-svg-core";
import { faShieldAlt } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

library.add(faShieldAlt);
Vue.component("font-awesome-icon", FontAwesomeIcon);
Vue.config.productionTip = false;

// include main css
import "@/assets/main.css";

new Vue({
  router,
  render: h => h(App)
}).$mount("#app");
