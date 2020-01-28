import { ResultSummary } from './ResultSummary';
import { ScanObjectType } from '../types/Enums';

export class ScanSummary {
    target: string;
    counters: ResultSummary;

    constructor(private type: ScanObjectType, name: string) {
        this.target = name;
        this.counters = new ResultSummary();
        this.counters.NoDatas = Math.round(Math.random() * 100);
        this.counters.Errors = Math.round(Math.random() * 100);
        this.counters.Warnings = Math.round(Math.random() * 100);
        this.counters.Successes = Math.round(Math.random() * 100);
    }
}