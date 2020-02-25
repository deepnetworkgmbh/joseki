import axios from "axios";
import { ContainerImageScan, VulnerabilityCounter } from "@/models";
import { ImageScan } from "@/models/ImageScan";
import { ImageScanGroup } from "@/models/ImageScanGroup";
import { VulnerabilityGroup, TargetGroup, ImageScanDetailModel } from "@/models/VulnerabilityGroup";
import { ScanSummary } from '../models/ScanSummary';
import { ScanObjectType } from '@/types/Enums';
import { InfrastructureOverview } from '../models/InfrastructureOverview';

import { ScoreService } from './ScoreService';

export class DataService {

  private get baseUrl() {
    if (process.env.NODE_ENV === 'production') {
      // prefix the api url because devServer is not being used in production
      // as devServer was wiring the api calls using vue.config.js
      // the cors must be adjusted on the Production server
      return process.env.VUE_APP_API_URL;
    }
    return ''
  }

  public async getOverviewData() {
    console.log(`[] calling api/kube/overview`);

    return axios
      .get(this.baseUrl + "/api/kube/overview/")
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("overview request finished."));
  }

  public async getContainerImagesData() {
    console.log(`[] calling api/container-images/`);
    return axios
      .get(this.baseUrl + "/api/container-images/")
      .then((response) => response.data.images)
      .catch((error) => console.log(error))
      .finally(() => console.log("container images request finished."));
  }

  public async getImageScanResultData(imageUrl: string) {
    const url = this.baseUrl + "/api/container-image/" +
      this.fixedEncodeURIComponent(imageUrl);
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("container images scan request finished."));
  }

  public regroupDataBySeverities(data: any): ImageScanDetailModel {

    let result = new ImageScanDetailModel();
    result.description = data.description
    result.scanResult = data.scanResult
    result.image = data.image

    try {

      for (let i = 0; i < data.targets.length; i++) {
        let target = new TargetGroup(data.targets[i].Target);

        for (let j = 0; j < data.targets[i].Vulnerabilities.length; j++) {
          let vulnerability = data.targets[i].Vulnerabilities[j];
          let index = target.vulgroups.findIndex((v) => v.Severity === vulnerability.Severity);
          if (index < 0) {
            let vulgroup = new VulnerabilityGroup(vulnerability.Severity);
            vulgroup.Count = 1;
            vulgroup.Order = ScoreService.getOrderBySeverity(vulnerability.Severity);
            vulgroup.CVEs.push(vulnerability);
            target.vulgroups.push(vulgroup);
          } else {
            target.vulgroups[index].CVEs.push(vulnerability);
            target.vulgroups[index].Count += 1;
          }
        }
        target.vulgroups.sort((a, b) => a.Order > b.Order ? -1 : a.Order < b.Order ? 1 : 0);
        result.targets.push(target);
      }

    } catch (e) {
      console.log(`error parsing image scan detail data ${e}`)
    }

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
    return encodeURIComponent(str).replace(/[!*]/g, function (c) {
      return "%" + c.charCodeAt(0).toString(16);
    });
  }

  public async getGeneralOverviewData(dateString: string = '') {
    console.log(`[] calling api/audits/overview (${dateString})`);
    let suffix = (dateString === '') ? '' : '?date=' + encodeURIComponent(dateString);

    return axios
      .get(this.baseUrl + "/api/audits/overview" + suffix)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("overview request finished."));
  }

  public async getGeneralOverviewDiffData(date1: string, date2: string) {
    console.log(`[] calling api/audits/overview/diff/`);
    let suffix = '?date1=' + encodeURIComponent(date1) + '&date2=' + encodeURIComponent(date2);
    return axios
      .get(this.baseUrl + "/api/audits/overview/diff" + suffix)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("overview diff request finished."));
  }

  public async getComponentHistoryData(id: string) {
    let suffix = '?id=' + encodeURIComponent(id);
    console.log(`[] calling api/audits/component/history` + suffix);
    return axios
      .get(this.baseUrl + "/api/audits/component/history" + suffix)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("component history request finished."));
  }

  public async getComponentDetailData(id: string, date: string) {
    let suffix = '?id=' + encodeURIComponent(id);
    if (date != undefined) {
      suffix += '&date=' + date;
    }
    console.log(`[] calling api/audits/component/detail` + suffix);
    return axios
      .get(this.baseUrl + "/api/audits/component/detail" + suffix)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("component history detail finished."));
  }

  public async getComponentDiffData(id: string, date1: string, date2: string) {
    console.log(`[] calling api/audits/component/diff/`);
    let suffix = '?id=' + id + '&date1=' + encodeURIComponent(date1) + '&date2=' + encodeURIComponent(date2);
    return axios
      .get(this.baseUrl + "/api/audits/component/diff" + suffix)
      .then((response) => response.data)
      .catch((error) => console.log(error))
      .finally(() => console.log("component diff request finished."));
  }

}
