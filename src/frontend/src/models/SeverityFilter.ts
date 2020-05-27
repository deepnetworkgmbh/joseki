import { CheckSeverity } from '.';

export class SeverityFilter {
    nodata: boolean = true;
    failed: boolean = true;
    warning: boolean = true;
    success: boolean = true;

    Check(result: CheckSeverity) : boolean {
        switch (result.toString()) {
            case 'Failed': return this.failed;
            case 'NoData': return this.nodata;
            case 'Warning': return this.warning;
            case 'Success': return this.success;
        }
        return false;
    }

    AllChecked(): boolean {
        return this.nodata && this.failed && this.warning && this.success;
    }

    Binary(): string[] {
        return [
            this.nodata ? 'NoData' : '',
            this.failed ? 'Failed' : '',
            this.warning ? 'Warning' : '',
            this.success ? 'Success' : ''
        ].filter(x => x !== '')
    }
}