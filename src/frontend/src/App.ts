import { Component, Vue, Watch } from "vue-property-decorator";
import Navigation from "@/components/navigation/Navigation.vue";

@Component({
  components: {
    Navigation
  }
})
export default class App extends Vue {

  sideWindowOpen: boolean = true;

  toggleSideWindow(isOpen: boolean) {
    this.sideWindowOpen = isOpen;
  }

  getWrapperClass() {
    const bareClass = 'container mx-auto '
    return bareClass + (this.sideWindowOpen ? 'wrapper-shring' : 'wrapper');
  }

  // always scroll to top when navigation occurs
  @Watch('$route.name')
  onRouteChanged() {
    var element = <HTMLElement>document.getElementById("nav");
    var top = element.offsetTop;
    window.scrollTo(0, top);
  }
}
