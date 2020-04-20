import { InfrastructureComponentSummary } from '@/models';
import { MappingService } from '@/services/MappingService';
import { DiffCollection, CheckCollection, DiffOperation, CheckObject, CheckControlGroup, CheckControl } from '@/services/DiffService';
import * as _ from 'lodash';
import { DateTime } from 'luxon';
import { CountersSummary } from './CounterSummary';

export class InfrastructureComponentDiff {
  /// Components of first summary.
  summary1: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Components of second summary.
  summary2: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Computed diff result
  results: DiffCollection[] = [];

  public static fromData(data): InfrastructureComponentDiff {

    let diff = new InfrastructureComponentDiff();
    diff.summary1 = data.summary1;
    diff.summary2 = data.summary2;
    diff.summary1.current = new CountersSummary(data.summary1.current)
    diff.summary2.current = new CountersSummary(data.summary2.current)

    let left: CheckCollection[] = MappingService.getResultsByCollection(diff.summary1.checks);
    let right: CheckCollection[] = MappingService.getResultsByCollection(diff.summary2.checks)
    diff.results = DiffCollection.CompareCollections(left, right);
  
    //  let checks = MappingService.getResultsByCollection(diff.summary1.checks);
    //  let objects = [checks[0].objects, checks[1].objects, checks[2].objects, checks[3].objects, checks[4].objects]; 

    //  let left: CheckCollection[] = DiffMock.GetCollection(true, objects.slice());
    //  let right: CheckCollection[] = DiffMock.GetCollection(false, objects.slice());
    //  diff.results = DiffCollection.CompareCollections(left, right);
    return diff;
  }

}

export class DiffMock {

  public static GetCollection(left: boolean, objects: CheckObject[][]): CheckCollection[] {
    let result: CheckCollection[] = []
    let date = new DateTime();

    if (left) {
      // removed collection
      let c0 = new CheckCollection("Removed Collection", "namespace", date);    
      c0.score = 68;
      c0.objects = objects[0].slice();
      result.push(c0);
    }

    let unchangedObject = objects[3][1];
    let changedObjectA = _.cloneDeep(objects.slice()[3].slice()[0]);
    let changedObjectB = _.cloneDeep(objects.slice()[3].slice()[0]);

    if (!left) {
      changedObjectB.controlGroups[0].items[0].result = "NoData";
      changedObjectB.controlGroups[0].items[0].icon = "fas fa-times nodata-icon";
    }
    
    // changed collection
    let c1 = new CheckCollection("Changed Collection", "namespace", date);    
    c1.score = left ? 40 : 60;
    c1.objects = left ? [changedObjectA, unchangedObject,...objects[1]] 
                      : [changedObjectB, unchangedObject,...objects[2]]
    result.push(c1);


    let c2 = new CheckCollection("Changed 2 Collection (+)", "namespace", date);
    c2.score = left ? 55: 65;
  
    let co1= new CheckObject();
    co1.id = 'aaaaa';
    co1.name ='aaaaa';
    co1.score = 50;
    co1.type = 'aaaaa';
    co1.controlGroups = [];
    
    let cg1  = new CheckControlGroup();
    cg1.name = 'aaaaa';
    cg1.items = []

    let cc1 = new CheckControl();
    cc1.id = 'first';
    cc1.text = 'first check';
    cc1.result = 'success'
    cc1.operation = DiffOperation.Same;
    cg1.items.push(cc1);

    if(!left) {
      let cc2 = new CheckControl();
      cc2.id = 'second';
      cc2.text = 'second check';
      cc2.result = 'success'
      cc2.operation = DiffOperation.Same;
      cg1.items.push(cc2);  
    }

    co1.controlGroups.push(cg1);
    c2.objects.push(co1)
    result.push(c2)


    if (!left) {
      // added collection
      let c2 = new CheckCollection("Added Collection", "namespace", date);    
      c2.score = 68;
      c2.objects = objects[4].slice();
      result.push(c2);
    }

    let c3 = new CheckCollection("Same Collection", "namespace", date);    
    c3.score = 100;
    c3.objects = objects[4].slice();    
    result.push(c3);

    return result;
  }

}

export function clone<T>(a: T): T {
  return JSON.parse(JSON.stringify(a));
}