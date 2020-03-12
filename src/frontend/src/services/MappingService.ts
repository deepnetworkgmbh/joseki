import { Check, CountersSummary, CheckSeverity, Collection } from '@/models'
import { DiffCounters } from '@/models/ComponentDiff'

export enum DiffOperation {
    Added = 'ADDED',
    Removed = 'REMOVED',
    Changed = 'CHANGED',
    Same = 'SAME'
}

export class CheckCollection {
    score: number = 0
    counters: CountersSummary = new CountersSummary()
    objects: CheckObject[] = []
    operation?: DiffOperation
    empty: boolean = false;

    constructor(public name: string, public type: string, public date: Date) {}

    public static GetEmpty() : CheckCollection {
        let empty = new CheckCollection("", "", new Date());
        empty.empty = true;
        return empty;
    }
    // compare objects
    Compare(other: CheckCollection): [DiffOperation, DiffCounters] {

        // parent Diff State
        let operation = DiffOperation.Same;
        let changes = new DiffCounters();

        if(this.score !== other.score) {
            operation = DiffOperation.Changed;
        }

        // compare this object array for removals
        for(let i=0; i<this.objects.length;i++) {
            let myObject = this.objects[i];
            let otherIndex = other.objects.findIndex(x=>x.id === myObject.id);
            if (otherIndex === -1) {
                // this object is removed on other scan
                operation = DiffOperation.Changed;                
                myObject.operation = DiffOperation.Removed;
                other.objects.push(CheckObject.GetEmpty(myObject.id))

                changes.tick(myObject.operation)
                continue;
            }
            // check object children
            other.objects[otherIndex].Compare(myObject);           
        }

        // compare other object array for addition
        for(let i=0; i<other.objects.length;i++) {
            let otherObject = other.objects[i];
            let myIndex = this.objects.findIndex(x=>x.id === otherObject.id);
            if (myIndex === -1) {
                // other obhect is removed from my scan
                otherObject.operation = DiffOperation.Added
                changes.tick(otherObject.operation)
                operation = DiffOperation.Changed;
                continue;
            }
            // check object children

            this.objects[myIndex].Compare(otherObject);           
            
        }  
        
        return [operation, changes];
    }
}

export class CheckObject {
    id: string = ''
    type: string = ''
    name: string = ''
    score: number = 0
    counters: CountersSummary = new CountersSummary()
    controls: CheckControl[] = []
    controlGroups: CheckControlGroup[] = []
    operation?: DiffOperation
    checked: boolean = false;
    empty: boolean = false;

    public static GetEmpty(id:string) : CheckObject {
        let empty = new CheckObject();
        empty.id = id;
        empty.empty = true;
        return empty;
    }

    Compare(other: CheckObject): DiffCounters {
        let changes = new DiffCounters();

        // check controlGroups
       
        // check for removals
        for(let j=0; j<this.controlGroups.length; j++) {
            let myControlGroup = this.controlGroups[j];
            let otherControlGroupIndex = other.controlGroups.findIndex(x=>x.name === myControlGroup.name);
            if(otherControlGroupIndex === -1) {
                this.operation = DiffOperation.Changed;
                myControlGroup.operation = DiffOperation.Removed;
                changes.tick(myControlGroup.operation);
                continue;
            }
            let otherControlGroup = other.controlGroups[otherControlGroupIndex];
            let [cgOperation, cgChanges] = otherControlGroup.Compare(myControlGroup);
            myControlGroup.operation = cgOperation;
            changes.merge(cgChanges);
        }

        // check for additions
        for(let j=0; j<other.controlGroups.length; j++) {
            let otherControlGroup = other.controlGroups[j];
            let myControlGroupIndex = this.controlGroups.findIndex(x=>x.name === otherControlGroup.name);
            if(myControlGroupIndex === -1) {
                this.operation = DiffOperation.Changed;
                otherControlGroup.operation = DiffOperation.Added;
                changes.tick(otherControlGroup.operation);
                continue;               
            }
            let myControlGroup = this.controlGroups[myControlGroupIndex];
            let [cgOperation, cgChanges] = myControlGroup.Compare(otherControlGroup);
            otherControlGroup.operation = cgOperation;
            changes.merge(cgChanges);
        }
        
        return changes;
    }
}

export class CheckControl {
    id: string = ''
    text: string = ''
    result: string = ''
    icon: string = ''
    order: number = 0
    tags: string[] = []
    operation?: DiffOperation
}

export class CheckControlGroup {
    name: string = ''
    items: CheckControl[] = []
    operation?: DiffOperation

    Compare(other: CheckControlGroup): [DiffOperation, DiffCounters] { 
        let operation = DiffOperation.Same
        let changes = new DiffCounters()

        for(let i=0;i< this.items.length; i++) {
            let myControl = this.items[i];
            let otherControlIndex = other.items.findIndex(x=>x.id === myControl.id);
            if(otherControlIndex === -1) {
                myControl.operation = DiffOperation.Removed;
                this.operation = DiffOperation.Changed;
                changes.tick(this.operation);
                continue;
            }
            let otherControl = other.items[otherControlIndex];            
            if(myControl.result !== otherControl.result || myControl.text !== otherControl.text) {
                myControl.operation = DiffOperation.Changed;
                this.operation = DiffOperation.Changed;
                changes.tick(this.operation);
                continue;
            }           
            myControl.operation = DiffOperation.Same;
        }

        for(let i=0;i< other.items.length; i++) {
            let otherControl = other.items[i];
            let myControlIndex = this.items.findIndex(x=>x.id === otherControl.id);
            if(myControlIndex === -1) {
                otherControl.operation = DiffOperation.Added;
                this.operation = DiffOperation.Changed;
                changes.tick(this.operation);
                continue;
            }
            let myControl = this.items[myControlIndex];            
            if(myControl.result !== otherControl.result || myControl.text !== otherControl.text) {
                otherControl.operation = DiffOperation.Changed;
                this.operation = DiffOperation.Changed;
                changes.tick(this.operation);
                continue;
            }
            myControl.operation = DiffOperation.Same;
        }

        return [operation, changes];
    }
}

export class MappingService {
    public static getResultsByCategory(checks: Check[]): any[] {
        var results: any[] = []

        // walk over all checks and group them by cateory.
        for (let i = 0; i < checks.length; i++) {
            const check = checks[i]

            if (results.findIndex(x => x.category === check.category) === -1) {
                results.push({
                    category: check.category,
                    counters: new CountersSummary(),
                })
            }

            const summaryIndex = results.findIndex(x => x.category === check.category)

            switch (check.result.toString()) {
                case 'Failed':
                    results[summaryIndex].counters.failed += 1
                    break
                case 'NoData':
                    results[summaryIndex].counters.noData += 1
                    break
                case 'Warning':
                    results[summaryIndex].counters.warning += 1
                    break
                case 'Success':
                    results[summaryIndex].counters.passed += 1
                    break
            }

            results[summaryIndex].counters.total += 1
        }
        //console.log(categoryCounters);

        for (let i = 0; i < results.length; i++) {
            results[i].score = results[i].counters.calculateScore()
        }

        results.sort((a, b) => (a.score < b.score ? 1 : a.score > b.score ? -1 : 0))


        return results
    }

    public static getResultsByCollection(checks: Check[]): CheckCollection[] {
        var results: CheckCollection[] = []

        // walk over all checks and group them by collections.
        for (let i = 0; i < checks.length; i++) {
            let check = checks[i];

            if (results.findIndex(x => x.name === check.collection.name) === -1) {
                let collection = new CheckCollection(check.collection.name, check.collection.type, check.date);
                results.push(collection)
            }

            const collectionIndex = results.findIndex(x => x.name === check.collection.name)

            if (results[collectionIndex].objects.findIndex(x => x.id === check.resource.id) === -1) {

                let checkObject = new CheckObject();
                checkObject.id = check.resource.id,
                checkObject.type = check.resource.type,
                checkObject.name = check.resource.name,
                checkObject.score = 0,
                checkObject.controls = [],           // for flat control list
                checkObject.controlGroups = [],      // for grouped control list
                checkObject.counters = new CountersSummary(),
                checkObject.checked = false

                results[collectionIndex].objects.push(checkObject)
            }

            const objectIndex = results[collectionIndex].objects.findIndex(x => x.id == check.resource.id);

            switch (check.result.toString()) {
                case 'Failed':
                    results[collectionIndex].counters.failed += 1
                    results[collectionIndex].objects[objectIndex].counters.failed += 1
                    break
                case 'NoData':
                    results[collectionIndex].counters.noData += 1
                    results[collectionIndex].objects[objectIndex].counters.noData += 1
                    break
                case 'Warning':
                    results[collectionIndex].counters.warning += 1
                    results[collectionIndex].objects[objectIndex].counters.warning += 1
                    break
                case 'Success':
                    results[collectionIndex].counters.passed += 1
                    results[collectionIndex].objects[objectIndex].counters.passed += 1
                    break
            }

            results[collectionIndex].counters.total += 1
            results[collectionIndex].objects[objectIndex].counters.total += 1

            let control = new CheckControl();
            control.id = check.control.id;
            control.text = check.control.message;
            control.result = check.result;
            control.icon = this.getControlIcon(check.result);
            control.order = this.getSeverityScore(check.result);
            control.tags = check.tags;

            if (check.tags.subGroup) {
                let groupIndex = results[collectionIndex].objects[objectIndex].controlGroups.findIndex(x => x.name === check.tags.subGroup)
                if (groupIndex === -1) {
                    let cg = new CheckControlGroup();
                    cg.name = check.tags.subGroup;
                    cg.items = [control];
                    results[collectionIndex].objects[objectIndex].controlGroups.push(cg);
                } else {
                    results[collectionIndex].objects[objectIndex].controlGroups[groupIndex].items.push(control);
                }
            } else {
                results[collectionIndex].objects[objectIndex].controls.push(control);
            }
        }

        // sort objects by severity
        for (let i = 0; i < results.length; i++) {
            results[i].score = results[i].counters.calculateScore()

            for (let j = 0; j < results[i].objects.length; j++) {
                results[i].objects[j].controls.sort((a, b) => (a.order < b.order ? -1 : a.order > b.order ? 1 : 0));
                results[i].objects[j].score = results[i].objects[j].counters.calculateScore();

                for (let k = 0; k < results[i].objects[j].controlGroups.length; k++) {
                    results[i].objects[j].controlGroups[k].items.sort((a, b) => (a.order < b.order ? -1 : a.order > b.order ? 1 : 0))
                }
            }
            results[i].objects.sort((a, b) => (a.score < b.score ? 1 : a.score > b.score ? -1 : 0))

        }

        // sort groups by name
        results.sort((a, b) => (a.score < b.score ? 1 : a.score > b.score ? -1 : 0))

        console.log(`[] collections result `, results)
        return results
    }

    public static getControlIcon(severity: CheckSeverity): string {
        switch (severity.toString()) {
            case 'NoData':
                return 'fas fa-times nodata-icon'
            case 'Failed':
            case 'Warning':
                return 'fas fa-exclamation-triangle warning-icon'
            case 'Success':
                return 'fas fa-check noissues-icon'
        }
        return ''
    }

    public static getSeverityScore(severity: CheckSeverity): number {
        switch (severity.toString()) {
            case 'NoData':
                return 10
            case 'Failed':
            case 'Warning':
                return 100
            case 'Success':
                return 1
        }
        return 0;
    }
}

