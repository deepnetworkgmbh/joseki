import { CountersSummary } from '@/models';
import { DateTime } from 'luxon';

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
                r[rowIndex].left!.SetChildren(DiffOperation.Removed);             
                continue;
            }      
            let right1 = right[rightIndex]; // right collection;
            row.right = right1;
            let [operation, changes] = right1.Compare(left1);      
            //r[rowIndex].changes.merge(changes); 
            //r[rowIndex].operation = operation;
        }
    
    
        // traverse second collections list
        for (let i = 0; i < right.length; i++) {
            let right1 = right[i];    //  right collection
            let key = right1.type + '---' + right1.name;
    
            let rowIndex = r.findIndex(x => x.key === key);
            if (rowIndex === -1) {
                let row = new DiffCollection(key, right1.name, right1.type);
                row.left = CheckCollection.GetEmpty();
                right1.SetChildren(DiffOperation.Added);             
                row.right = right1;
                row.operation = DiffOperation.Added;
                r.push(row);
                continue;
            }
    
            r[rowIndex].right = right1;
            let [operation, changes] = r[rowIndex].left!.Compare(right1);
            r[rowIndex].changes.merge(changes); 
            if(r[rowIndex].operation === DiffOperation.Same && operation !== DiffOperation.Same) {
                console.log(`[] operation`, operation)
                r[rowIndex].operation = operation;
            }
            //r[rowIndex].operation = DiffOperation.Changed;
        }
    
        // Sort results
    
        for(let i=0; i< r.length; i++) {
          let row = r[i];
          let left = row.left;
          let right = row.right;
          //if(left && left.objects.length>0) {
          left!.objects.sort(DiffCollection.OpSort);
          //}
          //if(right && right.objects.length>0) {
          right!.objects.sort(DiffCollection.OpSort);
          //}
        }
    
        // sort changed to top
        r.sort(DiffCollection.OpSort);
    
        return r;
    }

    public static RowSort(a, b) {
        if (a.empty === true && b.empty === false) return -1;
        if (a.empty === false && b.empty === true) return 1;
        return 0;
    }

    public static OpSort(a, b) {
        //if (DiffCollection.OpScore(a.operation) > DiffCollection.OpScore(b.operation)) return -1;
        //if (DiffCollection.OpScore(a.operation) < DiffCollection.OpScore(b.operation)) return 1;
        if (a.id > b.id) return -1;
        if (a.id < b.id) return 1;
        return 0;
    }

    public static OpScore(op: DiffOperation): number {
        switch (op) {
            case DiffOperation.Changed: return 80;
            case DiffOperation.Added : return 70;
            case DiffOperation.Removed : return 60;
            case DiffOperation.Same: 1;                            
        }
        return 0;
    }

}

export class CheckCollection {
    score: number = 0
    counters: CountersSummary = new CountersSummary(undefined)
    objects: CheckObject[] = []
    operation?: DiffOperation = DiffOperation.Same;
    empty: boolean = false;
    changes: DiffCounters = new DiffCounters();  
    checked: boolean = false;
    
    constructor(public name: string, public type: string, public owner: string, public date: DateTime) {}

    public static GetEmpty() : CheckCollection {
        let empty = new CheckCollection("", "", "", DateTime.fromJSDate(new Date()));
        empty.empty = true;
        return empty;
    }

    SetChildren(op:DiffOperation) {
        for(let i=0;i<this.objects.length;i++) {
            this.objects[i].operation = op;
            for(let j=0;j<this.objects[i].controlGroups.length;j++) {
                for(let k=0;k<this.objects[i].controlGroups[j].items.length;k++) {
                    this.objects[i].controlGroups[j].items[k].operation = op;
                }                
            }
            for(let j=0;j<this.objects[i].controls.length;j++) {
                this.objects[i].controls[j].operation = op;              
            }
        }
    }

    // compare objects
    Compare(other: CheckCollection): [DiffOperation, DiffCounters] {

        // parent Diff State
        let operation = DiffOperation.Same;
        let changes = new DiffCounters();

        // check removals
        for(let i=0; i<this.objects.length;i++) {
            let myObject = this.objects[i];
            
            let otherIndex = other.objects.findIndex(x=>x.id === myObject.id && x.empty === false);
            if (otherIndex === -1 && myObject.empty === false) {                // this object is removed on other scan
                operation = DiffOperation.Changed;                
                myObject.operation = DiffOperation.Removed;
                myObject.SetChildren(DiffOperation.Removed);
                other.objects.push(CheckObject.GetEmpty(myObject.id))
                changes.tick(myObject.operation)
            }
        }

        // check for addition
        for(let i=0; i<other.objects.length;i++) {
            let otherObject = other.objects[i];           
            let myIndex = this.objects.findIndex(x=>x.id === otherObject.id && x.empty === false);
            if (myIndex === -1 && otherObject.empty === false) {                // other object is removed from my scan
                otherObject.operation = DiffOperation.Added
                //otherObject.SetChildren(DiffOperation.Added);
                operation = DiffOperation.Changed;
                changes.tick(otherObject.operation)
            }
        }  

        // check for changes
        for(let i=0; i<other.objects.length;i++) {
            let otherObject = other.objects[i];           
            let myIndex = this.objects.findIndex(x=>x.id === otherObject.id && x.empty === false);
            if (myIndex === -1) continue;
             let [childOperation, childChanges] = this.objects[myIndex].Compare(otherObject);           
            if(childChanges.total > 0) {
                otherObject.operation = DiffOperation.Changed;
                operation = DiffOperation.Changed;
                changes.merge(childChanges)
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
    counters: CountersSummary = new CountersSummary(undefined)
    controls: CheckControl[] = []
    controlGroups: CheckControlGroup[] = []
    operation?: DiffOperation = DiffOperation.Same;
    checked: boolean = false;
    empty: boolean = false;
    owner: string = '';
    
    public static GetEmpty(id:string) : CheckObject {
        let empty = new CheckObject();
        empty.id = id;
        empty.empty = true;
        return empty;
    }

    SetChildren(op:DiffOperation) {
        // modify children if only parent is added or removed
        if(op !== DiffOperation.Added && op !== DiffOperation.Removed ) return;
        for(let j=0;j<this.controlGroups.length;j++) {
            for(let k=0;k<this.controlGroups[j].items.length;k++) {
                this.controlGroups[j].items[k].operation = op;
            }                
        }
        for(let j=0;j<this.controls.length;j++) {
            this.controls[j].operation = op;                
        }
    }

    Compare(other: CheckObject): [DiffOperation, DiffCounters] {
        let operation = DiffOperation.Same;
        let changes = new DiffCounters();

        // check controlGroups      
        if(this.controlGroups.length > 0 || other.controlGroups.length > 0) {

            // check for removals
            for(let j=0; j<this.controlGroups.length; j++) {
                let myControlGroup = this.controlGroups[j];
                let otherControlGroupIndex = other.controlGroups.findIndex(x=>x.name === myControlGroup.name);
                if(otherControlGroupIndex === -1) {
                    operation = DiffOperation.Changed;
                    myControlGroup.operation = DiffOperation.Removed;
                    changes.tick(myControlGroup.operation);
                }
            }

            // check for additions
            for(let j=0; j<other.controlGroups.length; j++) {
                let otherControlGroup = other.controlGroups[j];
                let myControlGroupIndex = this.controlGroups.findIndex(x=>x.name === otherControlGroup.name);
                if(myControlGroupIndex === -1) {
                    operation = DiffOperation.Changed;
                    otherControlGroup.operation = DiffOperation.Added;
                    changes.tick(otherControlGroup.operation);
                }
            }

            // check for changes
            for(let j=0; j<other.controlGroups.length; j++) {
                let otherControlGroup = other.controlGroups[j];
                let myControlGroupIndex = this.controlGroups.findIndex(x=>x.name === otherControlGroup.name);
                if(myControlGroupIndex === -1) continue;               
                let myControlGroup = this.controlGroups[myControlGroupIndex];
                let [cgOperation, cgChanges] = myControlGroup.Compare(otherControlGroup);
                if(cgOperation !== DiffOperation.Same && operation === DiffOperation.Same) {
                    operation = DiffOperation.Changed    
                    myControlGroup.operation = operation;
                    otherControlGroup.operation = operation;                
                }                
                changes.merge(cgChanges);
            }
        }

        // check controls
        if(this.controls.length > 0 || other.controls.length > 0) {
            
            // check for removals
            for(let i=0;i< this.controls.length; i++) {
                let myControl = this.controls[i];
                let otherControlIndex = other.controls.findIndex(x=>x.id === myControl.id);
                if(otherControlIndex === -1) {
                    myControl.operation = DiffOperation.Removed;
                    operation = DiffOperation.Changed;
                    changes.tick(myControl.operation);
                }
            }
    
            // check for additions
            for(let i=0;i< other.controls.length; i++) {
                let otherControl = other.controls[i];
                let myControlIndex = this.controls.findIndex(x=>x.id === otherControl.id);
                if(myControlIndex === -1) {
                    otherControl.operation = DiffOperation.Added;
                    operation = DiffOperation.Changed;
                    changes.tick(otherControl.operation);
                }
            }

            // check for changes
            for(let i=0;i< other.controls.length; i++) {
                let otherControl = other.controls[i];
                let myControlIndex = this.controls.findIndex(x=>x.id === otherControl.id);
                if(myControlIndex === -1) continue;
                let myControl = this.controls[myControlIndex];            
                if(myControl.result !== otherControl.result || myControl.text !== otherControl.text) {
                    myControl.operation = DiffOperation.Changed;
                    otherControl.operation = DiffOperation.Changed;
                    operation = DiffOperation.Changed;
                    changes.tick(otherControl.operation);
                }
            }
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
    operation?: DiffOperation = DiffOperation.Same;
}

export class CheckControlGroup {
    name: string = ''
    items: CheckControl[] = []
    operation?: DiffOperation = DiffOperation.Same;

    SetChildren(op:DiffOperation) {
        // modify children if only parent is added or removed
        if(op !== DiffOperation.Added && op !== DiffOperation.Removed ) return;
        for(let j=0;j<this.items.length;j++) {
            this.items[j].operation = op;                
        }
    }

    Compare(other: CheckControlGroup): [DiffOperation, DiffCounters] { 
        let operation = DiffOperation.Same
        let changes = new DiffCounters()

        // check for removals
        for(let i=0;i< this.items.length; i++) {
            let myControl = this.items[i];
            let otherControlIndex = other.items.findIndex(x=>x.id === myControl.id);
            if(otherControlIndex === -1) {
                myControl.operation = DiffOperation.Removed;
                operation = DiffOperation.Changed;
                changes.tick(operation);
            }
        }

        // check for additions
        for(let i=0;i< other.items.length; i++) {
            let otherControl = other.items[i];
            let myControlIndex = this.items.findIndex(x=>x.id === otherControl.id);
            if(myControlIndex === -1) {
                otherControl.operation = DiffOperation.Added;
                operation = DiffOperation.Changed;
                changes.tick(operation);
            }
        }

        // check for changes
        for(let i=0;i< other.items.length; i++) {
            let otherControl = other.items[i];
            let myControlIndex = this.items.findIndex(x=>x.id === otherControl.id);
            if(myControlIndex === -1) continue;
            let myControl = this.items[myControlIndex];            
            if(myControl.result !== otherControl.result || myControl.text !== otherControl.text) {
                myControl.operation = DiffOperation.Changed;
                otherControl.operation = DiffOperation.Changed;
                operation = DiffOperation.Changed;                
                changes.tick(operation);
            }
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
