import axios from "axios";
import { VulnerabilityGroup, TargetGroup, ImageScanDetailModel, InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponentDiff, InfrastructureOverviewDiff, MetaData, CountersSummary } from "@/models";
import { ScoreService } from './ScoreService';
import { DateTime } from 'luxon';
import { ConfigService } from './ConfigService';

export class DataService {

  private get baseUrl() {    
    return ConfigService.ApiUrl;
  }

  private get apiVersion():string {
    return '0.1'
  }

  public fixedEncodeURIComponent(str: string) {
    return encodeURIComponent(str).replace(/[!*]/g, function (c) {
      return "%" + c.charCodeAt(0).toString(16);
    });
  }

  public async getGeneralOverviewData(date?: DateTime): Promise<void | InfrastructureOverview> {

    let suffix = (date === undefined) ? '?api-version=' + this.apiVersion 
                                      : '?date=' + date!.toISODate() + '&api-version=' + this.apiVersion;

    let url = this.baseUrl + "/audits/overview" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => InfrastructureOverview.GenerateFromData(data))
      .catch((error) => console.log(error));
  }

  public async getComponentDetailData(id: string, date?: DateTime): Promise<void | InfrastructureComponentSummary> {

    let suffix = '?id=' + encodeURIComponent(id);
    if (date !== undefined) { suffix += '&date=' + date!.toISODate(); }
    suffix += '&api-version=' + this.apiVersion;   
    let url = this.baseUrl + "/audits/component/detail" + suffix;
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
      result.scoreHistory = result.scoreHistory.reverse(); //.slice(0, 14);
      result.current = new CountersSummary(data.current);
      console.log(`[] result`, result);
      return result;
    }

  }

  public async getImageScanResultData(imageTag: string, date: string): Promise<void | ImageScanDetailModel> {
    const suffix = this.fixedEncodeURIComponent(imageTag) + '/details/?date=' + date + '&api-version=' + this.apiVersion;
    const url = this.baseUrl + "/audits/container-image/" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    function processData(data: any): ImageScanDetailModel {
      console.log(`[] processing`, data);
      let result = new ImageScanDetailModel();
      result.date = data.date.split('T')[0];
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

  public async getGeneralOverviewDiffData(date1: string, date2: string): Promise<void | InfrastructureOverviewDiff> {
    let suffix = '?date1=' + date1 + '&date2=' + date2  + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/overview/diff" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => processData(data))
      .catch((error) => console.log(error));

    function processData(data:any) : InfrastructureOverviewDiff {
      console.log(`[]data`, JSON.parse(JSON.stringify(data)));
      let result = new InfrastructureOverviewDiff();
      result.summary1 = InfrastructureOverview.GenerateFromDiff(data.overall1, data.components1);
      result.summary2 = InfrastructureOverview.GenerateFromDiff(data.overall2, data.components2);
     
      // for(let i=0;i<result.summary1.components.length;i++) {
      //   let id = result.summary1.components[i].component.id;
      //   let index = result.summary2.components.findIndex(x=>x.component.id == id);
      //   if (index === -1) {
      //     // clone the component summary not to break the view
      //     result.summary2.components.push(result.summary1.components[i]);   
      //     // but mark as not loaded
      //     result.summary1.components[i].notLoaded = true;       
      //   }
      // }

      return result;
    }

  }

  public async getComponentHistoryData(id: string): Promise<void | InfrastructureComponentSummary[]> {
    let suffix = '?id=' + encodeURIComponent(id)  + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/component/history" + suffix;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => { 
        console.log(response.data);
        return response.data;
      })
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
    let suffix = '?id=' + encodeURIComponent(id) + '&date1=' + date1 + '&date2=' + date2 + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/component/diff" + suffix;
    console.log(`[] calling ${url}`)

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => {
        console.log(JSON.parse(JSON.stringify(data)));
        return InfrastructureComponentDiff.CreateFromData(data)
      })
      .catch((error) => console.log(error));

  }

  public async getWebsiteMeta(): Promise< void | MetaData[]> {
    let url = this.baseUrl + "/knowledgebase/website-metadata?api-version=" + this.apiVersion;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => data)
      .catch((error) => console.log(error));
  }

}
