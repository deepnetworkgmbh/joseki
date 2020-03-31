import { Check, CountersSummary, CheckSeverity, Collection } from '@/models'
import { CheckCollection, CheckControl, CheckControlGroup, CheckObject } from './DiffService'
import { DateTime } from 'luxon'
import { SeverityFilter } from '@/models/SeverityFilter'

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

    public static getResultsByCollection(checks: Check[], severityFilter: SeverityFilter): CheckCollection[] {
        var results: CheckCollection[] = []

        // walk over all checks and group them by collections.
        for (let i = 0; i < checks.length; i++) {
            let check = checks[i];

            if (!severityFilter.Check(check.result)) continue;

            if (results.findIndex(x => x.name === check.collection.name) === -1) {
                let date = DateTime.fromISO(check.date.toString());
                let collection = new CheckCollection(check.collection.name, check.collection.type, date);
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
                return 'icon-x-circle nodata-icon'
            case 'Failed':
                return 'icon-x-circle failed-icon'
            case 'Warning':
                return 'icon-alert-triangle warning-icon'
            case 'Success':
                return 'icon-check-circle noissues-icon'
        }
        return ''
    }

    public static getSeverityScore(severity: CheckSeverity): number {
        switch (severity.toString()) {
            case 'Success':
                return 1
            case 'NoData':
                return 10
            case 'Warning':
                return 50
            case 'Failed':
                return 100
        }
        return 0;
    }
}

