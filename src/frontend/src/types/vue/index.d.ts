declare module 'vue-msal' {
    import _Vue from 'vue';
    import { DataObject, Options } from 'vue-msal/lib/src/types';
  
    export class msalMixin extends Vue {
      readonly msal: DataObject
    }
  
    export default function MsalPlugin<Options>(Vue: typeof _Vue, options: Options): void
  }
