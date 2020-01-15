import { ContainerImageScan } from '.';
import { ImageScanGroup } from './ImageScanGroup'

export class ImageScan {
    scans: ContainerImageScan[] = []
    groups: ImageScanGroup[] = []
}