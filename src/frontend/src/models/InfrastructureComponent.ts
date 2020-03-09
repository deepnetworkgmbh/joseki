export class InfrastructureComponent {
  /// the id of the 
  id: string = ''

  /// The name of the component: dev-cluster, subscription-1, etc.
  name: string = ''

  /// The bucket of infrastructure component: Cloud Subscription, Kubernetes cluster, etc.
  category: string = ''

  sections: any[] = [];
}