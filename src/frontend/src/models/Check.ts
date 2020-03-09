import { Collection, CheckControl, CheckSeverity, Resource } from '@/models/';

export class Check {

  /// the date of the check.
  public date: Date = new Date();

  /// Name of the collection.
  /// kubernetes: namespace.
  /// azks: resource-group.
  public collection: Collection = new Collection();

  /// category of the check
  /// kubernetes: polaris/trivy category.
  /// azks: feature name.
  public category: string = '';

  /// The control name of the check.
  /// k8s: polaris `check` name.
  /// azks: azks `control` name.
  public control: CheckControl = new CheckControl();

  /// Result of the check.
  public result: CheckSeverity = CheckSeverity.NoData;

  /// Resource of the check.
  public resource: Resource = new Resource()

  /// Tag for check
  public tags: any;
}