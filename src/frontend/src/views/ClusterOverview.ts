import { Component, Vue } from "vue-property-decorator";
import { DataService} from "@/services/DataService";
import { ChartService } from "@/services/ChartService"
import { KubeOverview } from '@/models';
import Spinner from '@/components/spinner/Spinner.vue';

var Tabulator = require("tabulator-tables"); //import Tabulator library

@Component({
  components: {Spinner}
})
export default class ClusterOverview extends Vue {

  loaded: boolean = false;
  service:DataService = new DataService();
  data: KubeOverview = new KubeOverview();
  chartBoxStyle: google.visualization.ChartBoxStyle = {};
  tabulator: any;

  created() {

    this.service.getOverviewData()
      .then(response => {
        this.data = response;
        this.loaded = true;
        this.setupCharts();
      });
      window.addEventListener("resize", this.setupCharts);

  }

  destroyed() {
    window.removeEventListener("resize", this.setupCharts);
  }

  setupCharts(){
    google.load("visualization", "1", {packages: ["corechart"]});
    google.charts.load('current', {'packages':['corechart']});
    google.charts.setOnLoadCallback(this.drawCharts);
  }

  drawCharts(){
    ChartService.drawStackedChart(this.data.checkGroupSummary, (this.$refs.chart1 as HTMLInputElement))
    ChartService.drawStackedChart(this.data.namespaceSummary, (this.$refs.chart3 as HTMLInputElement))
    ChartService.drawPieChart(this.data.checkResultsSummary, (this.$refs.chart2 as HTMLInputElement))
    this.drawDataTable();
  }  

  drawDataTable() {
    let element = this.$refs.table as HTMLInputElement;    

    let tableOptions = {
      autoResize:true,
      height:300, 
      data:this.data.checks,
      layout:"fitColumns", 
      headerFilterPlaceholder:"...",
      tooltips:function(cell:any){
          //function should return a string for the tooltip of false to hide the tooltip
          const field = cell.getColumn().getField();
          if(field === 'id'){
              let desc = cell.getRow().getData().description;
              return desc;
          }
          return;
      },
      columns:[
          {   title:"Group", field:"group", width:100, align:'center',
              headerFilter:"select", headerFilterParams:{values:true },
          },
          {   title:"Category", field:"category", width:98, align:'center',
              headerFilter:"select", headerFilterParams:{values:true }
          },
          {   title:"Check Id", field:"id", width:200, align:'left',
              headerFilter:"select", headerFilterParams:{values:true}, formatter: function (cell:any) {
                  const value = cell.getValue();
                  const desc = cell.getData().description;
                  if(desc){
                      return value + '<i class="far fa-question-circle tip-icon"></i>' ;
                  }
                  return value;
              }
          },
          {   title:"Resource Name", field:"resourceName", headerFilter:"input" },
          {   title: "Result", field: "result", width: 77, align: 'center',
              headerFilter:"select", headerFilterParams:{values:true },
              formatter: function (cell:any, formatterParams:any) {
                  const val = cell.getValue();
                  return '<span class="tblr-' + val + '">' + val + '</span>';
              }
          },
          {
              title: "", width: 42, formatter: function () {
                  return '<button class="testbutton" onclick="openContextmenu(event)">...</button>';
              }
          }
      ],
      /* rowClick:function(e, row){ //trigger an alert message when the row is clicked
          alert("Row " + row.getData().id + " Clicked!!!!");
      },*/
      rowContext:function(e:any, row:any){
          e.preventDefault(); // prevent the browsers default context menu form appearing.
          //openContextmenu(e);
      },
  }

    this.tabulator = new Tabulator(element, tableOptions);
  }
}
