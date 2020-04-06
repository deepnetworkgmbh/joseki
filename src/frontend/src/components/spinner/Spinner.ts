import { Component, Vue, Prop } from "vue-property-decorator";

@Component
export default class Spinner extends Vue {

    @Prop()
    loadFailed: boolean = false;

    reloadClicked() {
        this.$emit('reload');
    }
}
