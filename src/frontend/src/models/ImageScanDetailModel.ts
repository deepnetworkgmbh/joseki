import { TargetGroup, VulnerabilityGroup } from '@/models';
import { ScoreService } from '@/services';

export class ImageScanDetailModel {
  date!: string;
  image!: string;
  scanResult!: string;
  description!: string;
  targets: TargetGroup[] = []

  public static fromData(data) : ImageScanDetailModel {
      let result = new ImageScanDetailModel();
      result.date = data.date.split('T')[0];
      result.description = data.description
      result.scanResult = data.scanResult
      result.image = data.image

      try {
        for (let i = 0; i < data.targets.length; i++) {
          let target = new TargetGroup(data.targets[i].target);

          for (let j = 0; j < data.targets[i].vulnerabilities.length; j++) {
            let vulnerability = data.targets[i].vulnerabilities[j];

            // split the references if it is not splitted correctly
            if (vulnerability.references.length === 1) {
              let newReferencesArray = vulnerability.references.slice()[0].split('\n');
              vulnerability.references = [];
              for (let r = 0; r < newReferencesArray.length - 1; r++) {
                vulnerability.references.push(newReferencesArray[r])
              }
            }

            let index = target.vulnerabilities.findIndex((v) => v.Severity === vulnerability.severity);
            if (index < 0) {
              let vulgroup = new VulnerabilityGroup(vulnerability.severity);
              vulgroup.Count = 1;
              vulgroup.Order = ScoreService.getOrderBySeverity(vulnerability.severity);
              vulgroup.CVEs.push(vulnerability);
              target.vulnerabilities.push(vulgroup);
            } else {
              target.vulnerabilities[index].CVEs.push(vulnerability);
              target.vulnerabilities[index].Count += 1;
            }
          }
          target.vulnerabilities.sort((a, b) => a.Order > b.Order ? -1 : a.Order < b.Order ? 1 : 0);
          result.targets.push(target);
        }
      } catch (e) {
        console.log(`error parsing image scan detail data ${e}`)
      }
      console.log(`[] result`, result);
      return result;
  }
}