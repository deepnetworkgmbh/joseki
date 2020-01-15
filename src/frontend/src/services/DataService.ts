import axios from "axios";
import { ContainerImageScan, VulnerabilityCounter } from "@/models";
import { ImageScan } from "@/models/ImageScan";
import { ImageScanGroup } from "@/models/ImageScanGroup";
import { VulnerabilityGroup } from "@/models/VulnerabilityGroup";

export class DataService {
  public async getOverviewData() {
    console.log(`[] calling api/kube/overview`);

    return axios
      .get("/api/kube/overview/")
      .then(function(response) {
        console.log(response);
        return response.data;
      })
      .catch(function(error) {
        console.log(error);
      })
      .finally(function() {
        console.log("overview request finished.");
      });
  }

  public async getContainerImagesData() {
    console.log(`[] calling api/container-images/`);

    return axios
      .get("/api/container-images/")
      .then(function(response) {
        console.log(response.data);
        return response.data.images;
      })
      .catch(function(error) {
        console.log(error);
      })
      .finally(function() {
        console.log("container images request finished.");
      });
  }

  public async getImageScanResultData(imageUrl: string) {
    const url = "/image/" + this.fixedEncodeURIComponent(imageUrl);
    console.log(`[] calling ${url}`);

    // can't continue as there is no live data
    // all current scans fail

    return axios
      .get(url)
      .then(function(response) {
        console.log(response);
        return response.data;
      })
      .catch(function(error) {
        console.log(error);
      })
      .finally(function() {
        console.log("container images request finished.");
      });
  }

  public regroupDataBySeverities(data: ContainerImageScan): VulnerabilityGroup {
    let result = new VulnerabilityGroup();
    // for (let i = 0; i < data.targets.length; i++) {

    // }

    return result;
  }
  public calculateImageSummaries(data: ContainerImageScan[]): ImageScan {
    let result = new ImageScan();
    result.scans = data;
    result.groups = [];

    let counters = [0, 0, 0, 0];

    for (let i = 0; i < data.length; i++) {
      const scan = data[i];
      if (scan.scanResult === "Succeeded") {
        if (scan.counters.length > 0) {
          let isCritical = false;
          for (let j = 0; j < scan.counters.length; j++) {
            const counter = scan.counters[j];
            if (
              counter.severity === "CRITICAL" ||
              counter.severity === "HIGH"
            ) {
              isCritical = true;
              break;
            }
          }
          if (isCritical) {
            counters[0] += 1;
          } else {
            counters[1] += 1;
          }
        } else {
          counters[2] += 1;
        }
      } else {
        counters[3] += 1;
      }
    }

    if (counters[0] > 0) {
      result.groups.push(
        new ImageScanGroup(
          counters[0],
          "CRITICAL",
          "with critical/high severity issues"
        )
      );
    }
    if (counters[1] > 0) {
      result.groups.push(
        new ImageScanGroup(
          counters[1],
          "MEDIUM",
          "with medium/low severity issues"
        )
      );
    }
    if (counters[2] > 0) {
      result.groups.push(
        new ImageScanGroup(counters[2], "NOISSUES", "without issues")
      );
    }
    if (counters[3] > 0) {
      result.groups.push(new ImageScanGroup(counters[3], "NODATA", "no data"));
    }
    return result;
  }

  public fixedEncodeURIComponent(str: string) {
    return encodeURIComponent(str).replace(/[!*]/g, function(c) {
      return "%" + c.charCodeAt(0).toString(16);
    });
  }
}
