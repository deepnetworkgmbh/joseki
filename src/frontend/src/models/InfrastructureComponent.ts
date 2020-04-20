export class InfrastructureComponent {
  id: string = ''
  name: string = ''
  category: string = ''
  sections: any[] = [];

  /**
   * returns icon for the component.
   *
   * @returns {string}
   * @memberof InfrastructureComponent
   */
  public static getIcon(category: string) : string {
    if(category === 'Azure Subscription') {
      return 'component-icon-azure icon-azuredevops';
    }
    if(category === 'Kubernetes') {
      return 'component-icon-kubernetes icon-kubernetes';
    }  
    return '';
  }
}