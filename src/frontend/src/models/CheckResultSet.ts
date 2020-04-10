import { OverviewCheck } from './Check';

export class CheckResultSet
{
        /// <summary>
        /// List of checks in resultset.
        /// </summary>
        public checks: OverviewCheck[] = [];

        /// <summary>
        /// Pagination size of the resultset.
        /// </summary>
        public pageSize!: number;

        /// <summary>
        /// Current pagination index of the resultset.
        /// </summary>
        public pageIndex!: number;

        /// <summary>
        /// Current resultset sorted by.
        /// </summary>
        public sortBy!: string;

        /// <summary>
        /// Number of total results in the result set.
        /// </summary>
        public totalResults!: number;
    }