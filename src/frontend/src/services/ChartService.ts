import { BarChartOptions, PieChartOptions, AreaChartOptions } from '@/types/';
import { CountersSummary, ScoreHistoryItem } from '@/models';
import { DateTime } from 'luxon';

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
			pieHole: 0.3,
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

	public static drawBarChart(
		data: ScoreHistoryItem[],
		key: string,
		selected: DateTime,
		cb?: Function,
		height: number = 100,
		selected2: DateTime = new DateTime(),
		gridlines: number = 0,
		componentId: string = '') {

		let element = document.getElementById(key) as any;

		var chart_data = new google.visualization.DataTable();
		chart_data.addColumn('date', 'X');
		chart_data.addColumn('number', 'Score');
		chart_data.addColumn({ type: 'string', role: 'style' });
		
		for (let i = 0; i < data.length; i++) {
			const rowdate = data[i].recordedAt.toString().split('T')[0];
			let isSelected = selected.toISODate() === rowdate || selected2.toISODate() === rowdate;
			let color = isSelected ? '#4AF0C0' : '#31B6A9';
			chart_data.addRow([new Date(rowdate), data[i].score, color]);
		}

		var options: BarChartOptions = {
			//width: 300,
			height: height,
			hAxis: {
				title: '',
				gridlines: { count: 0 },
				//gridlineColor: '#fff',
			},
			vAxis: {
				baselineColor: '#ccc',
				format: '',
				gridlineColor: '#eee',
				textPosition: gridlines === 0 ? 'none' : 'left',
				viewWindow: {
					max: 100,
					min: 0
				},
				gridlines: { count: gridlines }
			},
			series: {
				0: { color: '#41C6B9' }
			},
			backgroundColor: 'transparent',
			legend: { position: 'none' },
			orientation: 'horizontal',
			chartArea: {
				width: '85%',
				height: '90%'
			},
		};
		var chart = new google.visualization.BarChart(element);

		function selectHandler() {
			var selectedItem = chart.getSelection()[0];
			if (selectedItem && cb) {
				let row = selectedItem.row as number;
				var selectedDate = DateTime.fromJSDate(chart_data.getValue(row, 0)).toISODate(); 
				//console.log('[] selectedDate >', selectedDate);
				cb(selectedDate, componentId);				
			}
		}

		if (cb !== undefined) {
			google.visualization.events.addListener(chart, 'select', selectHandler);
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
