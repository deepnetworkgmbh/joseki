import { Check, CountersSummary, CheckSeverity } from '@/models/InfrastructureOverview'

export class DiffResult {
    name: string = ''
    type: string = ''
    objects: DiffObject[] = []
    score1: number = 0;
    score2: number = 0;
}

export class DiffObject {
    id: string = ''
    type: string = ''
    name: string = ''
    controls: DiffControl[] = []
    score1: number = 0;
    score2: number = 0;
}

export class DiffControl {
    id: string = ''
    text: string = ''
    icon1?: string = ''
    icon2?: string = ''
    result1: string = ''
    result2: string = ''
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

        return results
    }

    public static getResultsByCollection(checks: Check[]): any[] {
        var results: any[] = []

        // walk over all checks and group them by collections.
        for (let i = 0; i < checks.length; i++) {
            let row = checks[i]

            if (results.findIndex(x => x.name === row.collection.name) === -1) {
                results.push({
                    name: row.collection.name,
                    type: row.collection.type,
                    counters: new CountersSummary(),
                    score: 0,
                    objects: [],
                })
            }

            const collectionIndex = results.findIndex(x => x.name === row.collection.name)

            if (results[collectionIndex].objects.findIndex(x => x.id === row.resource.id) === -1) {
                results[collectionIndex].objects.push({
                    id: row.resource.id,
                    type: row.resource.type,
                    name: row.resource.name,
                    controls: [],
                    counters: new CountersSummary(),
                })
            }

            const objectIndex = results[collectionIndex].objects.findIndex(x => x.id == row.resource.id)

            switch (row.result.toString()) {
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

            results[collectionIndex].objects[objectIndex].controls.push({
                id: row.control.id,
                text: row.control.message,
                result: row.result,
                icon: this.getControlIcon(row.result),
                order: this.getSeverityScore(row.result),
            })
        }

        // sort objects by severity
        for (let i = 0; i < results.length; i++) {
            results[i].score = results[i].counters.calculateScore()

            for (let j = 0; j < results[i].objects.length; j++) {
                results[i].objects[j].controls.sort((a, b) => (a.order < b.order ? -1 : a.order > b.order ? 1 : 0));
                results[i].objects[j].score = results[i].objects[j].counters.calculateScore();
            }
        }

        // sort groups by name
        results.sort((a, b) => (a.name < b.name ? -1 : a.name > b.name ? 1 : 0))

        console.log(`[] collections result `, results)
        return results
    }

    public static getResultsDiff(checks1: Check[], checks2: Check[]): DiffResult[] {
        // added // removed // changed

        let results: DiffResult[] = []
        let results1 = this.getResultsByCollection(checks1)
        let results2 = this.getResultsByCollection(checks2)

        // traverse first collections list
        for (let i = 0; i < results1.length; i++) {
            let row = results1[i];

            let coll2Index = results2.findIndex(x => x.name === row.name)
            if (coll2Index === -1) continue;    // collection does not exist (removed), skip

            let row2 = results2[coll2Index];

            // traverse collection objects
            for (let j = 0; j < row.objects.length; j++) {
                let obj1 = row.objects[j];
                let obj2Index = results2[coll2Index].objects.findIndex(x => x.id === obj1.id);
                if (obj2Index === -1) continue;  // object does not exist (removed), skip
                let obj2 = results2[coll2Index].objects[obj2Index];

                // traverse thru object controls
                for (let k = 0; k < obj1.controls.length; k++) {
                    let control1 = obj1.controls[k];
                    let control2Index = results2[coll2Index].objects[obj2Index].controls.findIndex(x => x.id === control1.id)
                    if (control2Index === -1) continue; // control does not exist (removed), skip
                    let control2 = obj2.controls[control2Index];

                    if (control1.result === control2.result) {
                        continue;  // results are the same, skip
                    }

                    let colIndex = results.findIndex(x => x.name === row.name);
                    if (colIndex === -1) {
                        results.push({
                            name: row.name,
                            type: row.type,
                            objects: [],
                            score1: row.score,
                            score2: row2.score
                        })
                        colIndex = results.findIndex(x => x.name === row.name);
                    }

                    let objIndex = results[colIndex].objects.findIndex(x => x.id === obj1.id);
                    if (objIndex === -1) {
                        results[colIndex].objects.push({
                            id: obj1.id,
                            type: obj1.type,
                            name: obj1.name,
                            controls: [],
                            score1: obj1.score,
                            score2: obj2.score
                        });
                        objIndex = results[colIndex].objects.findIndex(x => x.id === obj1.id);
                    }

                    results[colIndex].objects[objIndex].controls.push({
                        id: control1.id,
                        text: control1.text,
                        icon1: this.getControlIcon(control1.result),
                        icon2: this.getControlIcon(control2.result),
                        result1: control1.result,
                        result2: control2.result
                    });
                }
            }
        }
        return results
    }

    public static getControlIcon(severity: CheckSeverity) {
        switch (severity.toString()) {
            case 'NoData':
                return 'fas fa-times nodata-icon'
            case 'Failed':
            case 'Warning':
                return 'fas fa-exclamation-triangle warning-icon'
            case 'Success':
                return 'fas fa-check noissues-icon'
        }
    }

    public static getSeverityScore(severity: CheckSeverity) {
        switch (severity.toString()) {
            case 'NoData':
                return 10
            case 'Failed':
            case 'Warning':
                return 100
            case 'Success':
                return 1
        }
    }
}

