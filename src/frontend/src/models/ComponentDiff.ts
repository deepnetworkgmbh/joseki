import { InfrastructureComponentSummary } from '@/models';
import { MappingService, CheckCollection, DiffOperation } from '@/services/MappingService';

export class InfrastructureComponentDiff {
  /// Components of first summary.
  summary1: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Components of second summary.
  summary2: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Computed diff result
  results: DiffCollection[] = [];

  public static CreateFromData(data): InfrastructureComponentDiff {

    let diff = new InfrastructureComponentDiff();
    diff.summary1 = data.summary1;
    diff.summary2 = data.summary2;
    diff.summary1.sections = InfrastructureComponentSummary.getSections(diff.summary1.current);
    diff.summary2.sections = InfrastructureComponentSummary.getSections(diff.summary2.current);

    let r: DiffCollection[] = [];
    let left: CheckCollection[] = MappingService.getResultsByCollection(diff.summary1.checks)
    let right: CheckCollection[] = MappingService.getResultsByCollection(diff.summary2.checks)
    
    // traverse first collections list
    for (let i = 0; i < left.length; i++) {
      let left1 = left[i];    //  left collection
      let key = left1.type + '---' + left1.name;
      let row = new DiffCollection(key, left1.name, left1.type);
      row.left = left1;
      row.right = CheckCollection.GetEmpty();
      r.push(row);

      let rowIndex = r.findIndex(x => x.key === key);
      let rightIndex = right.findIndex(x => x.name === left1.name && x.type === left1.type);
      if (rightIndex === -1) {
        r[rowIndex].operation = DiffOperation.Removed;
        continue;
      }      
      let right1 = right[rightIndex]; // right collection;
      let [operation, changes] = right1.Compare(left1);      
      r[rowIndex].changes.merge(changes); 
      if(operation !== DiffOperation.Same) {
        r[rowIndex].operation = DiffOperation.Changed;
      }
    }


    // traverse second collections list
    for (let i = 0; i < right.length; i++) {
      let right1 = right[i];    //  right collection
      let key = right1.type + '---' + right1.name;

      let rowIndex = r.findIndex(x => x.key === key);
      if (rowIndex === -1) {
        let row = new DiffCollection(key, right1.name, right1.type);
        row.left = CheckCollection.GetEmpty();
        row.right = right1;
        row.operation = DiffOperation.Added;
        r.push(row);
        continue;
      }

      r[rowIndex].right = right1;
      let [operation, changes] = r[rowIndex].left!.Compare(right1);
      //r[rowIndex].changes.merge(changes); 
      if(operation !== DiffOperation.Same) {
        r[rowIndex].operation = DiffOperation.Changed;
      }
    }


    for(let i=0; i< r.length; i++) {
      let row = r[i];
      let left = row.left;
      let right = row.right;
      if(left && left.objects.length>0) {
        left.objects.sort((a, b) => a.id > b.id ? -1 : a.id < b.id ? 1 : 0).reverse();
      }
      if(right && right.objects.length>0) {
        right.objects.sort((a, b) => a.id > b.id ? -1 : a.id < b.id ? 1 : 0).reverse();
      }
    }


    // sort changed to top
    r.sort((a, b) => a.operation > b.operation ? -1 : a.operation < b.operation ? 1 : 0).reverse();

    diff.results = r;
    return diff;

  }

}

export class DiffCollection {
  operation: DiffOperation = DiffOperation.Same;
  left: undefined | CheckCollection;
  right: undefined | CheckCollection;
  changes: DiffCounters = new DiffCounters();  
  constructor(public key: string, public name: string, public type:string) { }
}

export class DiffCounters {
  added: number = 0;
  removed: number = 0;
  changed: number = 0;

  public tick(operation:DiffOperation) {
    switch(operation) {
      case DiffOperation.Added: this.added+=1; break;
      case DiffOperation.Removed: this.removed+=1; break;
      case DiffOperation.Changed: this.changed+=1; break;
      case DiffOperation.Same: break;
    }
  }

  public merge(changes: DiffCounters) {
    this.added += changes.added;
    this.removed += changes.removed;
    this.changed += changes.changed;
  }

  public toString(): string {
    let out: string[] = [];
    if(this.added > 0) out.push(`${this.added} added`);
    if(this.removed > 0) out.push(`${this.removed} removed`);
    if(this.changed > 0) out.push(`${this.changed} changed`);
    return out.join(', ');
  }

  public get total(): number { return this.added + this.removed + this.changed}
}
