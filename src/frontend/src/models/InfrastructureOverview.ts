export class InfrastructureOverview {

    /// Overall infrastructure summary.
    overall: InfrastructureComponentSummary = new InfrastructureComponentSummary();

    /// Separate summary for each involved component.
    components: InfrastructureComponentSummary[] = [];
}

export class InfrastructureComponentSummary {

    // date of the summary
    date: string = '';

    // the component of the summary
    component: InfrastructureComponent = new InfrastructureComponent()

    /// Latest known check-result counters.
    current: CountersSummary = new CountersSummary()

    /// Holds Scores per last 30 days.
    /// If no data for a day - places 0.
    scoreHistory: ScoreHistoryItem[] = []

    /// Pre-calculated parameters for drawing trend line.
    scoreTrend: Trend = new Trend()

    sections: any[] = [];

    checks: Check[] = [];

    public static getSections(c: CountersSummary): any[] {
        let result: any[] = [];
        if (c.noData > 0) {
            result.push({ label: 'no data', value: c.noData, color: '#B7B8A8' });
        }
        if (c.failed > 0) {
            result.push({ label: 'error', value: c.failed, color: '#E33035' });
        }
        if (c.warning > 0) {
            result.push({ label: 'warning', value: c.warning, color: '#F8A462' });
        }
        if (c.passed > 0) {
            result.push({ label: 'success', value: c.passed, color: '#41C6B9' });
        }
        //console.log(`[] ${name}`, result);
        return result;
    }
}

export class InfrastructureComponent {
    /// the id of the 
    id: string = ''

    /// The name of the component: dev-cluster, subscription-1, etc.
    name: string = ''

    /// The bucket of infrastructure component: Cloud Subscription, Kubernetes cluster, etc.
    category: string = ''
}


export class InfrastructureOverviewDiff {
    /// First overall infrastructure summary.
    overall1: InfrastructureComponentSummary = new InfrastructureComponentSummary();

    /// Second overall infrastructure summary.
    overall2: InfrastructureComponentSummary = new InfrastructureComponentSummary();

    ///Components of first summary.
    components1: InfrastructureComponentSummary[] = [];

    ///Components of second summary.
    components2: InfrastructureComponentSummary[] = [];
}

/// Represents the score of component at a given date.
export class ScoreHistoryItem {
    constructor(public recordedAt: Date, public score: number) { }
}

export class Trend {
    /// The slope of the trend line.
    public slope: number = 0;
    /// The offset of the trend line.
    public offset: number = 0;

    /// Calculates Trend parameters for subset of score values.
    public static GetTrend(values: ScoreHistoryItem[]): Trend {
        var trend = new Trend();

        // if no values, trend is the origin - (0,0).
        if (values == null || values.length == 0) {
            return trend;
        }

        var n = values.length;
        var sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
        var now = new Date();

        for (let i = 0; i < values.length; i++) {
            var x = 30 - (new Date().getTime() - values[i].recordedAt.getTime()) / (1000 * 3600 * 24);
            var y = values[i].score;
            sumX += x;
            sumXX += x * x;
            sumY += y;
            sumXY += x * y;
        }

        trend.slope = ((n * sumXY) - (sumX * sumY)) / ((n * sumXX) - (sumX * sumX));
        trend.offset = (sumY - (trend.slope * sumX)) / n;

        return trend;
    }
}


/// <summary>
/// Represents short summary of check-result counters.
/// </summary>
export class CountersSummary {
    /// Amount of Passed checks.
    public passed: number = 0

    /// Amount of failed checks.
    public failed: number = 0

    /// Amount of Warnings.
    public warning: number = 0

    /// Amount of checks with no-data result: requires a manual verification or Joseki is not able to perform the check.
    public noData: number = 0

    /// Total checks amount.
    public total: number = 0;

    /// The audit score. It indicates how close the infrastructure is to known best-practices configuration.
    /// NoData checks are excluded, Passed and Failed has doubled weight.
    public score: number = 0;

}

export class Check {
    /// <summary>
    /// unique id of the check.
    /// </summary>
    public Id: string = '';

    /// <summary>
    /// the date of the check.
    /// </summary>
    public Date: Date = new Date();

    /// <summary>
    /// Name of the collection.
    /// kubernetes: namespace.
    /// azks: resource-group.
    /// </summary>
    public Collection: Collection = new Collection();

    /// <summary>
    /// category of the check
    /// kubernetes: polaris/trivy category.
    /// azks: feature name.
    /// </summary>
    public Category: string = '';

    /// <summary>
    /// The object to be checked.
    /// k8s: object (deployment, pod, service etc).
    /// azks: resource (keyvault etc).
    /// </summary>
    public Object: InfrastructureComponent = new InfrastructureComponent();

    /// <summary>
    /// The control name of the check.
    /// k8s: polaris `check` name.
    /// azks: azks `control` name.
    /// </summary>
    public Control: CheckControl = new CheckControl();

    /// <summary>
    /// Result of the check.
    /// </summary>
    public Result: CheckSeverity = CheckSeverity.NoData;

}

/// <summary>
/// Collection type and name of a check.
/// (eg: namespace: default, resource group: common).
/// </summary>
export class Collection {
    /// <summary>
    /// Type of the collection.
    /// </summary>
    public Type: string = '';

    /// <summary>
    /// Name of the collection.
    /// </summary>
    public Name: string = '';
}

/// <summary>
/// Control tag and description of a check.
/// (eg: namespace: default, resource group: common).
/// </summary>
export class CheckControl {
    /// <summary>
    /// Id of the control.
    /// </summary>
    public Id: string = '';

    /// <summary>
    /// Message of the control.
    /// </summary>
    public Message: string = '';
}

/// <summary>
/// Severity of the check.
/// </summary>
export enum CheckSeverity {
    /// <summary>
    /// Enum value when a scan was not found
    /// </summary>
    NoData = "NO_DATA",

    /// <summary>
    /// Enum value when a scan failed
    /// </summary>
    Failed = "FAILED",

    /// <summary>
    /// Enum value when a scan has warning
    /// </summary>
    Warning = "WARNING",

    /// <summary>
    /// Enum value when a scan succeeded
    /// </summary>
    Success = "SUCCESS",
}