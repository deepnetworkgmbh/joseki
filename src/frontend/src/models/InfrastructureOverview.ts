import { InfrastructureComponentSummary } from '@/models/';

export class InfrastructureOverview {

    /// Overall infrastructure summary.
    overall: InfrastructureComponentSummary = new InfrastructureComponentSummary();

    /// Separate summary for each involved component.
    components: InfrastructureComponentSummary[] = [];
}