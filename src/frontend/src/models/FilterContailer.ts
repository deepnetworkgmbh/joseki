import { Filter } from '@/components/filter/AdvancedFilter';

export class FilterContainer {

    filters: Filter[] = [];
    
    constructor(private channel: string, filterString: string) {
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

            if (channel === 'component' && filter.label === 'component') {
                filter.deletable = false;
            }
            
            this.filters.push(filter);
        }

        // take the component to first
        let componentIndex = this.filters.findIndex(x => x.label === 'component');
        if (componentIndex !== -1) {
            this.filters.splice(0, 0, this.filters.splice(componentIndex, 1)[0]);
        }
    }

    public isInFilter(label: string, value: string): boolean {
        let existingIndex = this.filters.findIndex(x=>x.label === label);
        if (existingIndex === -1) {
            return false;
        }
        return this.filters[existingIndex].values.indexOf(value) !== -1;
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
            }
        }else {
            let newFilter = new Filter();
            newFilter.label = label,
            newFilter.values = [value]
            this.filters.push(newFilter);
        }
    }

    public getFilterString(index: number = -1) : string {
        let result: string[] = [];
        let limit = index === -1 ? this.filters.length : index;
        for(let i=0; i<limit;i++) {
            result.push(this.filters[i].label + "=" + this.filters[i].values.join(','));
        }
        if(result.length === 0) return btoa('*');
        return btoa(result.join("&"));
    }
}