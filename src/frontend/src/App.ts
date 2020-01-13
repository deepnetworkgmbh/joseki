import { Component, Vue } from "vue-property-decorator";
import Navigation from "@/components/navigation/Navigation.vue";

@Component({
  components: {
    Navigation
  }
})
export default class App extends Vue {}
