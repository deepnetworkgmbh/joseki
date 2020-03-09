import { InfrastructureComponentSummary } from '@/models';
import { MappingService, CheckCollection, DiffOperation } from '@/services/MappingService';

export class InfrastructureComponentDiff {
  /// Components of first summary.
  summary1: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Components of second summary.
  summary2: InfrastructureComponentSummary = new InfrastructureComponentSummary();
  /// Computed diff result
  results: CheckCollection[] = [];

  public static CreateFromData(data): InfrastructureComponentDiff {
    let diff = new InfrastructureComponentDiff();
    diff.summary1 = data.summary1;
    diff.summary2 = data.summary2;
    diff.summary1.sections = InfrastructureComponentSummary.getSections(diff.summary1.current);
    diff.summary2.sections = InfrastructureComponentSummary.getSections(diff.summary2.current);

    let r: DiffCollection[] = [];
    let left = MappingService.getResultsByCollection(diff.summary1.checks)
    let right = MappingService.getResultsByCollection(diff.summary2.checks)

    // traverse first collections list
    for (let i = 0; i < left.length; i++) {
      let left1 = left[i];    //  left collection
      let key = left1.type + '---' + left1.name;
      let row = new DiffCollection(key);
      row.left = left1;
      r.push(row);

      let rowIndex = r.findIndex(x => x.key === key);

      let rightIndex = right.findIndex(x => x.name === left1.name && x.type === left1.type);
      if (rightIndex === -1) {
        r[rowIndex].operation = DiffOperation.Removed;
        continue;
      }
      let right1 = right[rightIndex]; // right collection;
      if (left1.score !== right1.score) {
        r[rowIndex].operation = DiffOperation.Changed;
      }
    }

    // traverse second collections list
    for (let i = 0; i < right.length; i++) {
      let right1 = right[i];    //  right collection
      let key = right1.type + '---' + right1.name;

      let rowIndex = r.findIndex(x => x.key === key);
      if (rowIndex === -1) {
        r.push({
          key: key,
          left: undefined,
          right: right1,
          operation: DiffOperation.Added
        })
        continue;
      }

      r[rowIndex].right = right1;
      let left1 = r[rowIndex].left;
      if (left1 !== undefined && left1.score !== right1.score) {
        r[rowIndex].operation = DiffOperation.Changed
      }
    }

    r.sort((a, b) => a.operation > b.operation ? -1 : a.operation < b.operation ? 1 : 0).reverse();

    console.log(r);
    return diff;

  }
}

export class DiffCollection {
  operation: DiffOperation = DiffOperation.Same;
  left: undefined | CheckCollection;
  right: undefined | CheckCollection;
  constructor(public key: string) { }
}

// let row = results1[i];

// let coll2Index = results2.findIndex(x => x.name === row.name)
// if (coll2Index === -1) {
//   row.operation = DiffOperation.Removed;
//   r.push(row);
//   continue;    // collection does not exist (removed), skip
// }

// let row2 = results2[coll2Index];

// // traverse collection objects
// for (let j = 0; j < row.objects.length; j++) {
//   let obj1 = row.objects[j];
//   let obj2Index = results2[coll2Index].objects.findIndex(x => x.id === obj1.id && x.type === obj1.type);
//   if (obj2Index === -1) {
//     row.operation = DiffOperation.Changed;
//     obj1.operation = DiffOperation.Removed;
//     row.objects.push(obj1);
//     continue;  // object does not exist (removed), skip
//   }
//   let obj2 = results2[coll2Index].objects[obj2Index];

//   // traverse thru object controls
//   for (let k = 0; k < obj1.controls.length; k++) {
//     let control1 = obj1.controls[k];
//     let control2Index = results2[coll2Index].objects[obj2Index].controls.findIndex(x => x.id === control1.id)
//     if (control2Index === -1) {
//       row.operation = DiffOperation.Changed;
//       obj1.operation = DiffOperation.Changed;
//       control1.operation = DiffOperation.Removed;
//       obj1.controls.push(control1);
//       continue; // control does not exist (removed), skip
//     }
//     let control2 = obj2.controls[control2Index];
//     if (control1.result === control2.result) {
//       row.operation = DiffOperation.Changed;
//       obj1.operation = DiffOperation.Changed;
//       control1.operation = DiffOperation.Changed;
//       obj1.controls.push(control1);
//       continue;  // results are the same, skip
//     }

//     // let colIndex = r.findIndex(x => x.name === row.name);
//     // if (colIndex === -1) {
//     //   r.push({
//     //     name: row.name,
//     //     type: row.type,
//     //     objects: [],
//     //     score1: row.score,
//     //     score2: row2.score,
//     //   })
//     //   colIndex = r.findIndex(x => x.name === row.name);
//     // }

//     // let objIndex = r[colIndex].objects.findIndex(x => x.id === obj1.id);
//     // if (objIndex === -1) {
//     //   r[colIndex].objects.push({
//     //     id: obj1.id,
//     //     type: obj1.type,
//     //     name: obj1.name,
//     //     controls: [],
//     //     score1: obj1.score,
//     //     score2: obj2.score
//     //   });
//     //   objIndex = r[colIndex].objects.findIndex(x => x.id === obj1.id);
//     // }

//     // r[colIndex].objects[objIndex].controls.push({
//     //   id: control1.id,
//     //   text: control1.text,
//     //   icon1: MappingService.getControlIcon(control1.result),
//     //   icon2: MappingService.getControlIcon(control2.result),
//     //   result1: control1.result,
//     //   result2: control2.result
//     // });
//   }
// }
// }

// diff.results = r;