import { DateTime } from 'luxon';
import { ChartService } from '@/services';

/// Represents the score of component at a given date.
export class ScoreHistoryItem {
  constructor(public recordedAt: string, public score: number) { }

  /**
   * Convert single score history serie into multiple series.
   * One is above threshold value, the other below.
   * 
   * @static
   * @param {ScoreHistoryItem[]} history
   * @memberof ScoreHistoryItem
   */
  public static getInterpolatedThresholdSeries(history: ScoreHistoryItem[]) {
    let series = [
      { name: 'success', data: ScoreHistoryItem.get(history, (score)=> { return score >= ChartService.successThresholdScore})},
      { name: 'warning', data: ScoreHistoryItem.get(history, (score)=> { return score < ChartService.successThresholdScore})},
    ];
    return ScoreHistoryItem.fillGaps(series);    
  }

  /**
   * Return a new series array out of the provided scoreHistoryItem list.
   * Filter out any value using the function provided.
   *
   * @private
   * @static
   * @param {ScoreHistoryItem[]} list
   * @param {Function} fn
   * @returns
   * @memberof ScoreHistoryItem
   */
  private static get(list: ScoreHistoryItem[], fn:Function) {    
    let result = list.map((i)=> ({ x: i.recordedAt.split('T')[0] , y: fn(i.score) ? i.score : null })).reverse();
    if (result.filter(x=>x.y !== null).length === 0 ) return [];
    return result;
  }

  /**
   * Interpolate two time serie values, fill the transition gaps.
   *
   * @static
   * @param {any[]} series
   * @returns {any[]}
   * @memberof ScoreHistoryItem
   */
  public static fillGaps(series: any[]): any[] {

    if(series[0].data.length === 0 || series[1].data.length === 0) return series;

    series[0].data = ScoreHistoryItem.interpolateSingle(series[0].data, series[1].data);
    series[1].data = ScoreHistoryItem.interpolateSingle(series[1].data, series[0].data);

    series[0].data = ScoreHistoryItem.interpolateFall(series[0].data, series[1].data);
    series[1].data = ScoreHistoryItem.interpolateFall(series[1].data, series[0].data);

    series[0].data = ScoreHistoryItem.interpolateRise(series[0].data, series[1].data);
    series[1].data = ScoreHistoryItem.interpolateRise(series[1].data, series[0].data);
   
    return series;
  }

  // interpolate [value -> null]
  private static interpolateFall(data0: any[], data1: any[]) : any[] {
    for(let i=0;i<data0.length-1;i++) {
      if(data0[i].y !== null && data0[i+1].y === null && data1[i].y === null && data1[i+1].y !== null) {
        data0[i+1] = data1[i+1];
      }
    }
    return data0.slice();
  }

  // interpolate [null -> value]
  private static interpolateRise(data0: any[], data1: any[]) : any[] {
    for(let i=0;i<data0.length-1;i++) {
      if(data0[i].y === null && data0[i+1].y !== null && data1[i].y !== null && data1[i+1].y === null) {
        data0[i] = data1[i];
      }
    }
    return data0.slice();
  }

  // interpolate [null -> value -> null]
  private static interpolateSingle(data0: any[], data1: any[]) : any[] {
    for(let i=1;i<data0.length-1;i++) {
      if(data0[i-1].y === null && data0[i].y !== null && data0[i+1].y === null && data1[i-1] !== null && data1[i+1] !== null) {
        data0[i-1].y = data1[i-1].y;
        data0[i+1].y = data1[i+1].y;       
      }
    }
    return data0.slice();
  }
}
