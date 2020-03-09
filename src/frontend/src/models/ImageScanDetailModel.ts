import { TargetGroup } from '@/models/';

export class ImageScanDetailModel {
  image: string = "";
  scanResult: string = "";
  description: string = "";
  targets: TargetGroup[] = []
}