import { CountersSummary } from '@/models';

export enum DiffOperation {
    Added = 'ADDED',
    Removed = 'REMOVED',
    Changed = 'CHANGED',
    Same = 'SAME'
}

export class DiffCollection {
    operation: DiffOperation = DiffOperation.Same;
    left: undefined | CheckCollection;
    right: undefined | CheckCollection;
    changes: DiffCounters = new DiffCounters();  
    checked: boolean = true;
    constructor(public key: string, public name: string, public type:string) { }

    public static CompareCollections(left: CheckCollection[], right:CheckCollection[]): DiffCollection[] {
        let r: DiffCollection[] = [];
    
        // traverse first collections list
        for (let i = 0; i < left.length; i++) {
            let left1 = left[i];    //  left collection
            let key = left1.type + '---' + left1.name;
            let row = new DiffCollection(key, left1.name, left1.type);
            row.left = left1;
            row.right = CheckCollection.GetEmpty();
            r.push(row);
    
            let rowIndex = r.findIndex(x => x.key === key);
            let rightIndex = right.findIndex(x => x.name === left1.name && x.type === left1.type && x.empty === false);
            if (rightIndex === -1) {
                r[rowIndex].operation = DiffOperation.Removed;
                continue;
            }      
            let right1 = right[rightIndex]; // right collection;
            row.right = right1;
            let [operation, changes] = right1.Compare(left1);      
            //r[rowIndex].changes.merge(changes); 
            r[rowIndex].operation = operation;
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
            r[rowIndex].changes.merge(changes); 
            //r[rowIndex].operation = DiffOperation.Changed;
        }
    
        // Sort results
    
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
        r.sort((a, b) => a.operation > b.operation ? -1 : a.operation < b.operation ? 1 : 0);
    
        return r;
    }
}

export class CheckCollection {
    score: number = 0
    counters: CountersSummary = new CountersSummary()
    objects: CheckObject[] = []
    operation?: DiffOperation
    empty: boolean = false;
    changes: DiffCounters = new DiffCounters();  

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
            let otherIndex = other.objects.findIndex(x=>x.id === myObject.id && x.empty === false);
            if (otherIndex === -1) {
                // this object is removed on other scan
                operation = DiffOperation.Changed;                
                myObject.operation = DiffOperation.Removed;
                other.objects.push(CheckObject.GetEmpty(myObject.id))
                changes.tick(myObject.operation)
                continue;
            }
            // check object children
            //other.objects[otherIndex].Compare(myObject);           
        }

        // compare other object array for addition
        for(let i=0; i<other.objects.length;i++) {
            let otherObject = other.objects[i];
            let myIndex = this.objects.findIndex(x=>x.id === otherObject.id && x.empty === false);
            if (myIndex === -1) {
                // other object is removed from my scan
                otherObject.operation = DiffOperation.Added
                changes.tick(otherObject.operation)
                operation = DiffOperation.Changed;
                continue;
            }
            // check object children

            let [childOperation, childChanges] = this.objects[myIndex].Compare(otherObject);           
            if(childOperation !== DiffOperation.Same) {
                this.changes = childChanges;                
            }
            
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

    Compare(other: CheckObject): [DiffOperation, DiffCounters] {
        let operation = DiffOperation.Same;
        let changes = new DiffCounters();

        // check controlGroups
       
        // check for removals
        for(let j=0; j<this.controlGroups.length; j++) {
            let myControlGroup = this.controlGroups[j];
            let otherControlGroupIndex = other.controlGroups.findIndex(x=>x.name === myControlGroup.name);
            if(otherControlGroupIndex === -1) {
                operation = DiffOperation.Changed;
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
                operation = DiffOperation.Changed;
                otherControlGroup.operation = DiffOperation.Added;
                changes.tick(otherControlGroup.operation);
                continue;               
            }
            let myControlGroup = this.controlGroups[myControlGroupIndex];
            let [cgOperation, cgChanges] = myControlGroup.Compare(otherControlGroup);
            otherControlGroup.operation = cgOperation;
            changes.merge(cgChanges);
        }
        
        return [operation, changes];
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
