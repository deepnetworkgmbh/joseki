import { ResultSummary } from './ResultSummary';
import { ScanObjectType } from '../types/Enums';

export class ScanSummary {
    target: string;
    counters: ResultSummary;
    total: number;
    subobjects: number = 0;

    constructor(private type: ScanObjectType, name: string) {
        this.target = name;
        this.counters = new ResultSummary();
        this.counters.NoDatas = Math.round(Math.random() * 5);
        this.counters.Errors = Math.round(Math.random() * 10);
        this.counters.Warnings = Math.round(Math.random() * 20);
        this.counters.Successes = 30 + Math.round(Math.random() * 100);
        this.total = this.counters.NoDatas + this.counters.Errors + this.counters.Warnings + this.counters.Successes;
        this.subobjects = 2 + Math.round(Math.random() * 10);
    }
    
    get value() {
        return Math.round(this.counters.Successes * 100 / this.total)
    }
    get sections() {
        let result: any[] = [];
        if(this.counters.NoDatas>0){
            result.push({ label: 'no data', value: this.counters.NoDatas, color: '#B7B8A8' });
        }
        if(this.counters.Errors>0){
            result.push({ label: 'error', value: this.counters.Errors, color: '#E33035' });
        }
        if(this.counters.Warnings>0){
            result.push({ label: 'warning', value: this.counters.Warnings, color: '#F8A462' });
        }
        if(this.counters.Successes>0){
            result.push({ label: 'success', value: this.counters.Successes, color: '#41C6B9' });
        }
        return result;
    }
}