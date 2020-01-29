import { ResultSummary } from '../models';
import { BarChartOptions, PieChartOptions, AreaChartOptions } from '../types/';
import { ImageScan } from '@/models/ImageScan';
import { mixins } from 'vue-class-component';

export class ChartService {
	public static groupColors = [ '#B7B8A8', '#E33035', '#F8A462', '#41C6B9' ];

	public static drawPieChart(summary: ResultSummary, element: HTMLInputElement, height: number = 320) {
		var data = google.visualization.arrayToDataTable([
			[ 'Severity', 'Number' ],
			[ 'No Data', Math.round(summary.NoDatas) ],
			[ 'Error', Math.round(summary.Errors) ],
			[ 'Warning', Math.round(summary.Warnings) ],
			[ 'Success', Math.round(summary.Successes) ]
		]);

		var options: PieChartOptions = {
			title: summary.resultName,
			titlePosition: 'none',
			//width:400,
			height: height,
			slices: {
				0: { color: this.groupColors[0] },
				1: { color: this.groupColors[1] },
				2: { color: this.groupColors[2] },
				3: { color: this.groupColors[3] }
			},
			pieHole: 0.5,
			chartArea: { top: 50, width: '100%', height: '80%' },
			legend: {
				position: 'top',
				alignment: 'center',
				textStyle: {
					fontSize: 9
				}
			}
		};

		var chart = new google.visualization.PieChart(element);
		chart.draw(data, options);
	}

	public static drawStackedChart(summary: ResultSummary[], element: HTMLInputElement) {
		if (summary.length === 0) return;
		let rawdata: any[][] = [ [ 'Severity', 'No Data', 'Error', 'Warning', 'Success' ] ];

		for (let i = 0; i < summary.length; i++) {
			const group = summary[i];
			rawdata.push([ group.resultName, group.NoDatas, group.Errors, group.Warnings, group.Successes ]);
		}

		// Set chart options
		let options: BarChartOptions = {
			isStacked: 'percent',

			//width:280,
			height: 250,
			series: {
				0: { color: this.groupColors[0] },
				1: { color: this.groupColors[1] },
				2: { color: this.groupColors[2] },
				3: { color: this.groupColors[3] }
			},
			legend: {
				position: 'top',
				alignment: 'center',
				textStyle: {
					fontSize: 9
				}
			},
			chartArea: { width: '100%', height: '80%' },
			hAxis: {
				textStyle: {
					color: 'gray',
					fontSize: 9
				},
				textPosition: 'out'
			},
			orientation: 'horizontal'
		};

		// Instantiate and draw our chart, passing in some options.
		let chart = new google.visualization.BarChart(element);
		let data = google.visualization.arrayToDataTable(rawdata);
		chart.draw(data, options);
	}

	public static drawSeverityPieChart(summary: ImageScan, element: HTMLInputElement, height: number = 250) {
		let jdata: any[][] = [ [ 'Severity', 'Number' ] ];
		let sevcolors: any = [];
		for (let i = 0; i < summary.groups.length; i++) {
			let group = summary.groups[i];
			jdata.push([ group.title, group.count ]);
			sevcolors.push(this.getSeverityColor(group.title));
		}

		let data = google.visualization.arrayToDataTable(jdata);
		let options: PieChartOptions = {
			titlePosition: 'none',
			//width: 400,
			height: height,
			slices: {
				0: { color: sevcolors[0] },
				1: { color: sevcolors[1] },
				2: { color: sevcolors[2] },
				3: { color: sevcolors[3] }
			},
			pieHole: 0.5,
			chartArea: { width: '100%', height: '70%' },
			legend: {
				position: 'bottom',
				alignment: 'center',
				textStyle: {
					fontSize: 9
				}
			}
		};

		let chart = new google.visualization.PieChart(element);
		chart.draw(data, options);
	}

	public static drawAreaChart(summary: ResultSummary, element: HTMLInputElement) {
		var data = new google.visualization.DataTable();
		data.addColumn('number', 'X');
		data.addColumn('number', 'Score');

		data.addRows([			
			[42, 63], [43, 66], [44, 67], [45, 69], [46, 69], [47, 70],
			[48, 72], [49, 68], [50, 66], [51, 65], [52, 67], [53, 70],
			[54, 71], [55, 72], [56, 73], [57, 75], [58, 70], [59, 68],
			[60, 64], [61, 60], [62, 65], [63, 67], [64, 68], [65, 69],
			[66, 70], [67, 72], [68, 75], [69, 80]
		]);

		var options: AreaChartOptions = {
			//width: 300,
			height: 100,
			hAxis: {
				title: '',
				gridlines: { count: 0 },
				baselineColor: '#fff',
				//gridlineColor: '#fff',
			},
			vAxis: {
				baselineColor: '#fff',
				//gridlineColor: '#fff',
				//textPosition: 'none',
				viewWindow: {
					max: 100,
					min: 0
				},
				gridlines: { count: 0 }
			},
			series: {
				0: { color: '#41C6B9' }
			},
			backgroundColor: 'white',
			legend: { position: 'none' },
			chartArea: { 
				width: '100%', 
				height: '100%'
			},
		};

		var chart = new google.visualization.AreaChart(element);
		chart.draw(data, options);
	}

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
}
