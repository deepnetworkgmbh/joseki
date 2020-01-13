import { ResultSummary } from '../models';
import { BarChartOptions } from '../types/BarChartOptions';
import { PieChartOptions } from '../types/PieChartOptions';

export class ChartService {
	public static groupColors = [ 'gray', 'red', 'orange', 'green' ];

	public static drawPieChart(summary: ResultSummary, element: HTMLInputElement) {
		var data = google.visualization.arrayToDataTable([
			[ 'Severity', 'Number' ],
			[ 'No Data', summary.NoDatas ],
			[ 'Error', summary.Errors ],
			[ 'Warning', summary.Warnings ],
			[ 'Success', summary.Successes ]
		]);

		var options: PieChartOptions = {
			title: summary.resultName,
			titlePosition: 'none',
			//width:320,
			height: 250,
			slices: {
				0: { color: this.groupColors[0] },
				1: { color: this.groupColors[1] },
				2: { color: this.groupColors[2] },
				3: { color: this.groupColors[3] }
			},
			pieHole: 0.5,
			chartArea: { width: '100%', height: '70%' },
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
}
