import axios from "axios";
import { VulnerabilityGroup, TargetGroup, ImageScanDetailModel, InfrastructureOverview, InfrastructureComponentSummary, InfrastructureComponentDiff, InfrastructureOverviewDiff, MetaData, CountersSummary, InfrastructureComponent } from "@/models";
import { ScoreService } from './ScoreService';
import { DateTime } from 'luxon';
import { ConfigService } from './ConfigService';
import { CheckResultSet } from '@/models/CheckResultSet';

export class DataService {

  /**
   * Return base url for api access.
   *
   * @readonly
   * @private
   * @memberof DataService
   */
  private get baseUrl() {
    return ConfigService.ApiUrl;
  }

  /**
   * Return current api version.
   *
   * @readonly
   * @private
   * @type {string}
   * @memberof DataService
   */
  private get apiVersion(): string {
    return '0.1'
  }

  /**
   * Return general overview data.
   *
   * @param {DateTime} [date]
   * @returns {(Promise<void | InfrastructureOverview>)}
   * @memberof DataService
   */
  public async getGeneralOverviewData(date?: DateTime): Promise<void | InfrastructureOverview> {

    let suffix = (date === undefined) ? '?api-version=' + this.apiVersion
      : '?date=' + date!.toISODate() + '&api-version=' + this.apiVersion;

    let url = this.baseUrl + "/audits/overview" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => InfrastructureOverview.GenerateFromData(data))
  }

  /**
   * Return general overview detail.
   *
   * @param {number} pageSize
   * @param {number} pageIndex
   * @param {DateTime} [date]
   * @param {string} [filterBy]
   * @param {string} [sortBy]
   * @returns {(Promise<void | CheckResultSet>)}
   * @memberof DataService
   */
  public async getGeneralOverviewDetail(pageSize: number, pageIndex: number, date?: DateTime, filterBy?: string, sortBy?: string): Promise<void | CheckResultSet> {
    let suffix = (date === undefined) ? '?api-version=' + this.apiVersion
      : '?date=' + date!.toISODate() + '&api-version=' + this.apiVersion;

    if (filterBy && filterBy.length > 0) {
      suffix += '&filterBy=' + filterBy;
    } else {
      suffix += '&filterBy=*';
    }
    if (sortBy && sortBy.length > 0) {
      suffix += '&sortBy=' + sortBy;
    }
    suffix += '&pageSize=' + pageSize;
    suffix += '&pageIndex=' + pageIndex;

    let url = this.baseUrl + "/audits/overview/detail/" + suffix;
    console.log(`[] calling ${url}`);

    return axios.get(url)
      .then((response) => response.data)
      .then((data) => <CheckResultSet>data);
  }

  /**
   * Return general overview search/filtering data.
   *
   * @param {DateTime} [date]
   * @param {string} [filterBy]
   * @returns {(Promise<void | any>)}
   * @memberof DataService
   */
  public async getGeneralOverviewSearch(date?: DateTime, filterBy?: string): Promise<void | any> {
    let suffix = (date === undefined) ? '?api-version=' + this.apiVersion
      : '?date=' + date!.toISODate() + '&api-version=' + this.apiVersion;

    if (filterBy && filterBy.length > 0) {
      suffix += '&filterBy=' + filterBy;
    } else {
      suffix += '&filterBy=*';
    }
    let url = this.baseUrl + "/audits/overview/search/" + suffix;
    console.log(`[] calling ${url}`);
    return axios.get(url)
      .then((response) => response.data)
  }

  /**
   * Return component detail data.
   *
   * @param {string} id
   * @param {DateTime} [date]
   * @returns {(Promise<void | InfrastructureComponentSummary>)}
   * @memberof DataService
   */
  public async getComponentDetailData(id: string, date?: DateTime): Promise<void | InfrastructureComponentSummary> {

    let suffix = '?id=' + encodeURIComponent(id);
    if (date !== undefined) { suffix += '&date=' + date!.toISODate(); }
    suffix += '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/component/detail" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => InfrastructureComponentSummary.fromData(data))
  }

  /**
   * Return image scan result data.
   *
   * @param {string} imageTag
   * @param {string} date
   * @returns {(Promise<void | ImageScanDetailModel>)}
   * @memberof DataService
   */
  public async getImageScanResultData(imageTag: string, date: string): Promise<void | ImageScanDetailModel> {
    const suffix = this.fixedEncodeURIComponent(imageTag) + '/details/?date=' + date + '&api-version=' + this.apiVersion;
    const url = this.baseUrl + "/audits/container-image/" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => ImageScanDetailModel.fromData(data))
  }

  /**
   * Return general overview diff data.
   *
   * @param {string} date1
   * @param {string} date2
   * @returns {(Promise<void | InfrastructureOverviewDiff>)}
   * @memberof DataService
   */
  public async getGeneralOverviewDiffData(date1: string, date2: string): Promise<void | InfrastructureOverviewDiff> {
    let suffix = '?date1=' + date1 + '&date2=' + date2 + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/overview/diff" + suffix;
    console.log(`[] calling ${url}`);

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => InfrastructureOverviewDiff.fromData(data))
  }

  /**
   * Return component history data.
   *
   * @param {string} id
   * @returns {(Promise<void | InfrastructureComponentSummary[]>)}
   * @memberof DataService
   */
  public async getComponentHistoryData(id: string): Promise<void | InfrastructureComponentSummary[]> {
    let suffix = '?id=' + encodeURIComponent(id) + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/component/history" + suffix;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data.reverse())
      .then((data) => <InfrastructureComponentSummary[]>data)
  }

  /**
   * Return component diff data.
   *
   * @param {string} id
   * @param {string} date1
   * @param {string} date2
   * @returns {(Promise<void | InfrastructureComponentDiff>)}
   * @memberof DataService
   */
  public async getComponentDiffData(id: string, date1: string, date2: string): Promise<void | InfrastructureComponentDiff> {
    let suffix = '?id=' + encodeURIComponent(id) + '&date1=' + date1 + '&date2=' + date2 + '&api-version=' + this.apiVersion;
    let url = this.baseUrl + "/audits/component/diff" + suffix;
    console.log(`[] calling ${url}`)

    return axios
      .get(url)
      .then((response) => response.data)
      .then((data) => InfrastructureComponentDiff.fromData(data));
  }

  /**
   * Return data from knowledgebase.
   *
   * @returns {(Promise< void | MetaData[]>)}
   * @memberof DataService
   */
  public async getKnowledgebaseData(ids: string[]): Promise<void | any[]> {
    let params = ids.map((v, i)=> `ids[${i}]=${v}`).join('&');
    let url = this.baseUrl + "/knowledgebase/items?" + params + "&api-version=" + this.apiVersion;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => response.data );
  }

  /**
   * Return website meta data.
   *
   * @returns {(Promise< void | MetaData[]>)}
   * @memberof DataService
   */
  public async getWebsiteMeta(): Promise<void | MetaData[]> {
    let url = this.baseUrl + "/knowledgebase/website-metadata?api-version=" + this.apiVersion;
    console.log(`[] calling ${url}`);
    return axios
      .get(url)
      .then((response) => <MetaData[]>response.data);
  }

  public fixedEncodeURIComponent(str: string) {
    return encodeURIComponent(str).replace(/[!*]/g, function (c) {
      return "%" + c.charCodeAt(0).toString(16);
    });
  }

}
