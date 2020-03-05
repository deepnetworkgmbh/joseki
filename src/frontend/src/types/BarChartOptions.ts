export interface BarChartOptions extends google.visualization.BarChartOptions {
    orientation: string;
    vAxis?: vAxis;
}

export interface vAxis extends google.visualization.ChartAxis {
    gridlineColor: string;
}
