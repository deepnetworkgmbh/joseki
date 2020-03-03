import { Vulnerability } from "./InfrastructureOverview";

export class ScanTarget {
  target: string = "";
  vulnerabilities: Vulnerability[] = [];
}
