import { ResultSummary } from '../models';
import { BarChartOptions, PieChartOptions, AreaChartOptions } from '../types/';
import { ImageScan } from '@/models/ImageScan';
import { mixins } from 'vue-class-component';
import { CountersSummary, ScoreHistoryItem } from '@/models/InfrastructureOverview';
import { ScoreService } from './ScoreService';

export class ChartService {
	public static groupColors = ['#B7B8A8', '#E33035', '#F8A462', '#41C6B9'];

	public static drawPieChart(summary: CountersSummary, key: string, height: number = 320) {

		let element = document.getElementById(key) as any;

		var data = google.visualization.arrayToDataTable([
			['Severity', 'Number'],
			['No Data', Math.round(summary.noData)],
			['Failed', Math.round(summary.failed)],
			['Warning', Math.round(summary.warning)],
			['Success', Math.round(summary.passed)]
		]);

		var options: PieChartOptions = {
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
			chartArea: { top: 50, width: '100%', height: '70%' },
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
		let rawdata: any[][] = [['Severity', 'No Data', 'Error', 'Warning', 'Success']];

		for (let i = 0; i < summary.length; i++) {
			const group = summary[i];
			rawdata.push([group.resultName, group.NoDatas, group.Errors, group.Warnings, group.Successes]);
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
		let jdata: any[][] = [['Severity', 'Number']];
		let sevcolors: any = [];
		for (let i = 0; i < summary.groups.length; i++) {
			let group = summary.groups[i];
			jdata.push([group.title, group.count]);
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

	public static drawBarChart(
		data: ScoreHistoryItem[],
		key: string,
		selected: Date,
		cb?: Function,
		height: number = 100,
		selected2: string = '') {

		let element = document.getElementById(key) as any;

		var chart_data = new google.visualization.DataTable();
		chart_data.addColumn('date', 'X');
		chart_data.addColumn('number', 'Score');
		chart_data.addColumn({ type: 'string', role: 'style' });

		let selectedDate = new Date(selected);
		let selected2Date = (selected2 == '') ? new Date() : new Date(selected2);

		for (let i = 0; i < data.length; i++) {
			const rowdate = new Date(data[i].recordedAt);
			const diff: any = selectedDate.getTime() - rowdate.getTime();
			const diff2: any = (selected2 == '') ? 1 : (selected2Date.getTime() - rowdate.getTime());
			let color = (diff === 0 || diff2 === 0) ? '#4AF0C0' : '#31B6A9';
			chart_data.addRow([rowdate, data[i].score, color]);
		}

		var options: BarChartOptions = {
			//width: 300,
			height: height,
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
			backgroundColor: 'transparent',
			legend: { position: 'none' },
			orientation: 'horizontal',
			chartArea: {
				width: '100%',
				height: '100%'
			},
		};
		var chart = new google.visualization.BarChart(element);

		function selectHandler() {
			var selectedItem = chart.getSelection()[0];

			console.log('[] selectedItem ', selectedItem);

			if (selectedItem && cb) {
				let row = selectedItem.row as number;
				var selectedDate = chart_data.getValue(row, 0);
				cb(selectedDate);
			}
		}

		if (cb) {
			google.visualization.events.addListener(chart, 'select', cb);
		}
		chart.draw(chart_data, options);
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
