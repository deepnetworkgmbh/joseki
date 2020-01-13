import { ClusterSummary } from './ClusterSummary';
import { ResultSummary } from './ResultSummary';
import { Check } from './Check';

export class KubeOverview {
    cluster: ClusterSummary = new ClusterSummary()
    checkGroupSummary: ResultSummary[] = []
    namespaceSummary: ResultSummary[] = []
    checkResultsSummary: ResultSummary = new ResultSummary()  
    checks:Check[] = []
}