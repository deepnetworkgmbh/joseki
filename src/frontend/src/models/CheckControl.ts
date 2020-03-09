/// <summary>
/// Control tag and description of a check.
/// (eg: namespace: default, resource group: common).
/// </summary>
export class CheckControl {
  /// <summary>
  /// Id of the control.
  /// </summary>
  public id: string = '';

  /// <summary>
  /// Message of the control.
  /// </summary>
  public message: string = '';
}