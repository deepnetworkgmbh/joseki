import { CountersSummary, ScoreHistoryItem } from '@/models';
import { DateTime } from 'luxon';

export class ChartService {
	public static groupColors = ['#B7B8A8', '#E33035', '#F8A462', '#41C6B9'];

	private static getSeverityColor(severity: string) {
		switch (severity) {
			case 'CRITICAL':
				return ChartService.groupColors[1];
			case 'MEDIUM':
				return ChartService.groupColors[2];
			case 'NOISSUES':
				return ChartService.groupColors[3];
			case 'NODATA':
				return ChartService.groupColors[0];
		}
	}


	private static get animationOptions() {
		return {
			enabled: true,
			easing: 'linear',
			speed: 10,
			dynamicAnimation: {
				enabled: true,
				speed: 150
			}
		}
	}

	private static get noAnimation() {
		return {
			enabled: false
		}
	}

	public static AreaChartOptions(id:string,scoreHistory: ScoreHistoryItem[], dates: DateTime[], scores: number[], cb: Function) : ApexCharts.ApexOptions {

		const xAxisAnnotations: any[] = [];
		xAxisAnnotations.push({
			x: new Date(dates[0].toISODate()).getTime(),
			borderWidth: 1,
			borderColor: '#775DD0',
			label: {
				borderWidth:0,
				style: {              
					background: 'transparent',
					color: '#444',
					fontSize: '9px'
				},
				text: `${scores[0]} %`
			},
		})
		if (dates.length === 2 && scores.length === 2) {
			xAxisAnnotations.push({
				x: new Date(dates[1].toISODate()).getTime(),
				borderWidth: 1,
				borderColor: '#775DD0',
				label: {
					borderWidth:0,
					style: {              
						background: 'transparent',
						color: '#444',
						fontSize: '9px'
					},
					text: `${scores[1]} %`
				},
			})	
		}

		return <ApexCharts.ApexOptions>{
			chart: {
				id: id,
				type: 'area',
				sparkline: { enabled: true },
				events: {
					dataPointSelection: function(event, chartContext, config) {
						//console.log(event, chartContext, config);
					},
					selection: function(chartContext, { xaxis, yaxis }) {
						//console.log(chartContext, xaxis, yaxis);
					},
					click: function(event, chartContext, config) {
						//console.log(event, chartContext, config);					
					},
					markerClick: function(event, chartContext, { seriesIndex, dataPointIndex, config}) {
						//console.log(event, chartContext, seriesIndex, dataPointIndex, config);	
						//console.log(scoreHistory[dataPointIndex].recordedAt);
						const index = scoreHistory.length - dataPointIndex - 1;
						const datestr = scoreHistory[index].recordedAt.split('T')[0];
						scores[0] = scoreHistory[index].score;
						if(scores[0]>0) {
							// dont call on score=0
							cb(datestr, scores[0]);						
						}
					}
				},
				animations: ChartService.animationOptions
			},
			stroke: { width: 1 },
			yaxis: {
				type: 'numeric',
				labels: {
					formatter: (value) => { return value + "%" },
					show: false,
				}        
			},
			xaxis: {
				type: 'datetime',
				crosshairs: { width: 1 },
			},
			tooltip: {
				fixed: {
					enabled: false
				},
				x: {
					show: true
				},
				y: {
					title: {
						formatter: function (value) {
							return 'Score'
						}
					}
				},
				marker: {
					show: false
				}
			},
			grid: {
				row: {
					colors: ['#e5e5e5', 'transparent'],
					opacity: 0.5
				}, 
				xaxis: {
					lines: {
						show: true
					}
				}
			},
			annotations: {
				xaxis: xAxisAnnotations
			}
		}

	}

	public static DonutChartOptions(id:string, summary: CountersSummary) : ApexCharts.ApexOptions {

		return <ApexCharts.ApexOptions>{
			chart: {
				type: 'donut',
				sparkline: {
					enabled: true
				},
				dropShadow: {
					enabled: true,
					top: 0,
					left: 0,
					blur: 2,
					opacity: 0.1
				},
				animations: ChartService.animationOptions
			},
			labels: summary.getLabels(),
			colors: summary.getColors(),			  
			stroke: {
				width: 1
			},
			tooltip: {
				fixed: {
					enabled: false
				},
			},
			plotOptions: {
				pie: {
					donut: {
						labels: {
							show: true,
							showAlways: true,
							name: {
								show: true,
								offsetY: 5
							},
							value: {
								show: false
							},
							total: {
								show: true,
								showAlways: true,
								label: summary.score + '%',
								fontSize: '13px'
							}
						}
					}
				}
			}
		}
	}

	public static PieChartOptions(id:string, summary: CountersSummary) : ApexCharts.ApexOptions {

		return <ApexCharts.ApexOptions>{
			chart: {
				type: 'pie',
				dropShadow: {
					enabled: true,
					top: 10,
					left: 10,
					blur: 10,
					opacity: 0.1
				},
				animations: ChartService.animationOptions
			},
			labels: summary.getLabels(),
			colors: summary.getColors(),
			plotOptions: {
				pie: {
					expandOnClick: false, 
					offsetX: 45,
					customScale: 0.8,
					dataLabels: {
						offset:-10
					}
				},
			},
			stroke: {
				show: true,
				width: 1
			}
		}

	}

	public static DiffAreaChartOptions(id:string, labels: string[]) : ApexCharts.ApexOptions {
		
		return <ApexCharts.ApexOptions>{
			chart: {
				id: id,
				type: 'area',
				stacked: true,
				toolbar: false,
				animations: ChartService.noAnimation
			},
			fill: {
				type: 'solid',
				opacity: 1
			},
			labels: labels,
			stroke: { width: 1 },
			yaxis: {
				show: false
			}
		}
	}
}
