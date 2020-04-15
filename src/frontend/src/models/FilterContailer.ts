import { Filter } from '@/components/filter/AdvancedFilter';

export class FilterContainer {

    filters: Filter[] = [];

    constructor(filterString: string) {
        console.log(`[fc] creating new container from string ${filterString}`);
        this.filters = [];
        let filterStr = atob(filterString);
        if (filterStr.trim().length === 0 || filterStr === '*') return;
        let arr = filterStr.split('&');
        for(let i=0; i < arr.length; i++) {
            let obj = arr[i].split('=');
            let filter = new Filter();
            filter.label = obj[0];
            filter.values = obj[1].split(',');
            console.log(`[fc] [${obj[0]}] =`, obj[1]);
            this.filters.push(filter);
        }
    }

    public removeFilterByIndex(index: number) {
        this.filters.splice(index, 1);
    }

    public removeFilterLabel(label: string) {
        let existingIndex = this.filters.findIndex(x=>x.label === label);
        if (existingIndex !== -1) {
            this.filters.splice(existingIndex, 1);
        }
    }

    public removeFilterValue(label: string, value: string) {
        let existingIndex = this.filters.findIndex(x=>x.label === label);
        if (existingIndex !== -1) {
            let valueIndex = this.filters[existingIndex].values.findIndex(v=> v === value);
            if (valueIndex !== -1) {
                this.filters[existingIndex].values.splice(valueIndex, 1);
                // remove the label if that was the last value
                if(this.filters[existingIndex].values.length === 0) {
                    this.filters.splice(existingIndex, 1);
                }
            }        
        }
    }

    public addFilter(label: string, value: string) {
        let existingIndex = this.filters.findIndex(x=>x.label === label);
        if(existingIndex !== -1) {
            //check if value exists
            let existingValueIndex = this.filters[existingIndex].values.indexOf(value);
            if(existingValueIndex === -1) {
                this.filters[existingIndex].values.push(value)
            }else{
                console.log(`value ${value} already exists in ${label}`);
            }
        }else {
            let newFilter = new Filter();
            newFilter.label = label,
            newFilter.values = [value]
            this.filters.push(newFilter);
        }
    }

    public getFilterString() : string {
        let result: string[] = [];
        for(let i=0; i<this.filters.length;i++) {
            result.push(this.filters[i].label + "=" + this.filters[i].values.join(','));
        }
        if(result.length === 0) return btoa('*');
        return btoa(result.join("&"));
    }
}