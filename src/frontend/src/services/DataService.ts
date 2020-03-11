import axios from "axios";
import { VulnerabilityGroup, TargetGroup, ImageScanDetailModel, InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponentDiff } from "@/models";
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

  public fixedEncodeURIComponent(str: string) {
    return encodeURIComponent(str).replace(/[!*]/g, function (c) {
      return "%" + c.charCodeAt(0).toString(16);
    });
  }

  public async getGeneralOverviewData(dateString: string = ''): Promise<void | InfrastructureOverview> {
    let suffix = (dateString === '') ? '' : '?date=' + encodeURIComponent(dateString);
    let url = this.baseUrl + "/api/audits/overview" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    function processData(data): InfrastructureOverview {
      let result = new InfrastructureOverview();
      result.overall = data.overall;

      // reverse and slice overall history
      result.overall.scoreHistory = result.overall.scoreHistory.reverse().slice(0, 14);
      result.components = data.components;

      // generate sections for components
      for (let i = 0; i < result.components.length; i++) {

        if (result.components[i].component.category === 'Subscription') {
          result.components[i].component.category = 'Azure Subscription';
        }
        result.components[i].sections = InfrastructureComponentSummary.getSections(result.components[i].current);
        result.components[i].scoreHistory = result.components[i].scoreHistory.reverse().slice(0, 14);

        // truncate scan times from component history
        for (let j = 0; j < result.components[i].scoreHistory.length; j++) {
          let inputDate = result.components[i].scoreHistory[j].recordedAt.toString();
          let dateReplacement = new Date(inputDate.split('T')[0] + 'T00:00:00');
          result.components[i].scoreHistory[j].recordedAt = dateReplacement;
        }
      }
      console.log("[] result", result);
      return result;
    }
  }

  public async getComponentDetailData(id: string, date: string = ''): Promise<void | InfrastructureComponentSummary> {
    let suffix = '?id=' + encodeURIComponent(id);
    if (date !== '' && date.indexOf('1970') === -1) { suffix += '&date=' + encodeURIComponent(date); }
    let url = this.baseUrl + "/api/audits/component/detail" + suffix;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    // TODO: move this method to InfrastructureComponentSummary() 
    function processData(data): InfrastructureComponentSummary {
      let result = <InfrastructureComponentSummary>data;
      if (result.component.category === 'Subscription') {
        result.component.category = 'Azure Subscription';
      }
      result.sections = InfrastructureComponentSummary.getSections(result.current);
      result.scoreHistory = result.scoreHistory.reverse().slice(0, 14);

      // truncate scan times from component history
      for (let j = 0; j < result.scoreHistory.length; j++) {
        let inputDate = result.scoreHistory[j].recordedAt.toString();
        let dateReplacement = new Date(inputDate.split('T')[0] + 'T00:00:00');
        result.scoreHistory[j].recordedAt = dateReplacement;
      }
      console.log(`[] result`, result);
      return result;
    }

  }

  public async getImageScanResultData(imageTag: string, date: string): Promise<void | ImageScanDetailModel> {
    const suffix = this.fixedEncodeURIComponent(imageTag) + '/details/?date=' + encodeURIComponent(date);
    const url = this.baseUrl + "/api/audits/container-image/" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    function processData(data: any): ImageScanDetailModel {
      console.log(`[] processing`, data);
      let result = new ImageScanDetailModel();
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

  public async getGeneralOverviewDiffData(date1: string, date2: string) {
    let suffix = '?date1=' + encodeURIComponent(date1) + '&date2=' + encodeURIComponent(date2);
    let url = this.baseUrl + "/api/audits/overview/diff" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .catch((error) => console.log(error));
  }

  public async getComponentHistoryData(id: string): Promise<void | InfrastructureComponentSummary[]> {
    let suffix = '?id=' + encodeURIComponent(id);
    let url = this.baseUrl + "/api/audits/component/history" + suffix;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error))
      .finally(() => console.log("component history request finished."));

    function processData(data): InfrastructureComponentSummary[] {
      let result = data.reverse();
      console.log(`[] result`, result);
      return result;
    }
  }

  public async getComponentDiffData(id: string, date1: string, date2: string): Promise<void | InfrastructureComponentDiff> {
    let suffix = '?id=' + id + '&date1=' + encodeURIComponent(date1) + '&date2=' + encodeURIComponent(date2);
    let url = this.baseUrl + "/api/audits/component/diff" + suffix;
    console.log(`[] calling ${url}`)

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    function processData(data): InfrastructureComponentDiff {
      return InfrastructureComponentDiff.CreateFromData(data);      
    }
  }


}
