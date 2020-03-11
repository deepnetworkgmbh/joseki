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

    constructor(public name: string, public type: string, public date: Date) {}

    // compare objects
    Compare(other: CheckCollection): [DiffOperation, DiffCounters] {

        // parent Diff State
        let result = DiffOperation.Same;
        let changes = new DiffCounters();

        if(this.score !== other.score) {
            result = DiffOperation.Changed;
        }

        // compare this object array for removals
        for(let i=0; i<this.objects.length;i++) {
            let myObject = this.objects[i];
            let otherIndex = other.objects.findIndex(x=>x.id === myObject.id);
            if (otherIndex === -1) {
                // this object is removed on other scan
                result = DiffOperation.Changed;                
                myObject.operation = DiffOperation.Removed;
                changes.tick(myObject.operation)
                continue;
            }
            // TODO: check object children

            // if no change, mark it as same
            myObject.operation = DiffOperation.Same;
        }

        // compare other object array for addition
        for(let i=0; i<other.objects.length;i++) {
            let otherObject = other.objects[i];
            let myIndex = this.objects.findIndex(x=>x.id === otherObject.id);
            if (myIndex === -1) {
                // other obhect is removed from my scan
                otherObject.operation = DiffOperation.Added
                changes.tick(otherObject.operation)
                result = DiffOperation.Changed;
                continue;
            }
            // TODO: check object children
            
                  // if no change, mark it as same
            otherObject.operation = DiffOperation.Same;
        }  
        
        return [result, changes];
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
                results[collectionIndex].objects.push({
                    id: check.resource.id,
                    type: check.resource.type,
                    name: check.resource.name,
                    score: 0,
                    controls: [],           // for flat control list
                    controlGroups: [],      // for grouped control list
                    counters: new CountersSummary(),
                    checked: false
                })
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
                    results[collectionIndex].objects[objectIndex].controlGroups.push({
                        name: check.tags.subGroup,
                        items: [control]
                    })
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

