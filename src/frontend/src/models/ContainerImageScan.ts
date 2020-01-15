import { VulnerabilityCounter } from "./VulnerabilityCounter";
import { ScanTarget } from "./ScanTarget";

export class ContainerImageScan {
  // json values
  image: string = "";
  scanResult: string = "";
  description: string = "";
  counters: VulnerabilityCounter[] = [];
  attributes: string[] = [];
  pods: string[] = [];
  targets: ScanTarget[] = [];
  // runtime variables
  rowText: string = "";
  order: number = 0;
  shortImageName: string = "";
  icon: string = "";
  iconColor: string = "";
  link: string = "";
  tip: string = "";
}
