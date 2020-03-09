import { VulnerabilityGroup } from '@/models';

export class TargetGroup {
  vulnerabilities: VulnerabilityGroup[] = [];
  constructor(public target: string) { }
}
