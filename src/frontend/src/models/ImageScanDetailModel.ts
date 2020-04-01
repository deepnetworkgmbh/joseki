import { TargetGroup } from '@/models';

export class ImageScanDetailModel {
  date!: string;
  image!: string;
  scanResult!: string;
  description!: string;
  targets: TargetGroup[] = []
}