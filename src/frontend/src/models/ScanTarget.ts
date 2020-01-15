import { Vulnerability } from "./Vulnerability";

export class ScanTarget {
  target: string = "";
  vulnerabilities: Vulnerability[] = [];
}
