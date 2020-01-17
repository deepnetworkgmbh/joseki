import { Component, Vue } from "vue-property-decorator";
import { DataService } from "@/services/DataService";
import { ChartService } from "@/services/ChartService";
import { ContainerImageScan, NamespaceCounters } from "@/models";
import { ImageScan } from "@/models/ImageScan";
import Spinner from "@/components/spinner/Spinner.vue";
import StatusBar from "@/components/statusbar/StatusBar.vue";

class Result {
  title: string = "";
  images: ContainerImageScan[] = [];
  order: number = 0;
  counter: NamespaceCounters = new NamespaceCounters();
}

@Component({
  components: { Spinner, StatusBar }
})
export default class ImageOverview extends Vue {
  service: DataService = new DataService();
  data: ImageScan = new ImageScan();
  loaded: boolean = false;
  groupBy: any[] = [];
  groupByOptions: HTMLOptionElement[] = [];
  selectedAttribute: string = "namespace";
  results: Result[] = [];
  filter: string = '';

  created() {
    this.service.getContainerImagesData().then(response => {
      this.data = this.service.calculateImageSummaries(response);
      this.loaded = true;
      console.log(this.data);
      this.setupPage();
    });    
  }

  setupPage() {
    this.createAttributeGroup();
    this.setupChart();
    this.renderList();
  }

  setupChart() {
    google.load("visualization", "1", { packages: ["corechart"] });
    google.charts.load("current", { packages: ["corechart"] });
    google.charts.setOnLoadCallback(this.drawCharts);
  }

  drawCharts() {
    ChartService.drawSeverityPieChart(
      this.data,
      this.$refs.chart1 as HTMLInputElement
    );
  }

  createAttributeGroup() {
    this.groupBy = [];
    this.groupByOptions = [];

    for (let i = 0; i < this.data.scans.length; i++) {
      const image = this.data.scans[i];

      for (let j = 0; j < image.attributes.length; j++) {
        const attribute = image.attributes[j];
        const chunk = attribute.split(":");
        const name = chunk[0];
        const attributeIndex = this.groupBy.findIndex(e => e.name === name);
        if (attributeIndex === -1) {
          this.groupBy.push({
            name: name,
            count: 1
          });
        } else {
          this.groupBy[attributeIndex].count += 1;
        }
      }
    }
    this.groupBy
      .sort((a, b) => (a.count < b.count ? -1 : a.count > b.count ? 1 : 0))
      .reverse();

    for (let i = 0; i < this.groupBy.length; i++) {
      const optionName =
        this.groupBy[i].name + " (" + this.groupBy[i].count + ")";
      const optionValue = this.groupBy[i].name;
      this.groupByOptions.push(new Option(optionName, optionValue));
    }
  }

  getSeveritFromImage(image: ContainerImageScan, severity: string) {
    let severityIndex = image.counters.findIndex(o => o.severity === severity);
    if (severityIndex === -1) {
      return 0;
    }
    return image.counters[severityIndex].count;
  }

  parseSeverities(image: ContainerImageScan) : { text: string, score: number} {
    let imageRowResult = {
      text: '',
      score: 0
    }
    
    let counters = {
      CRITICAL: 0,
      MEDIUM: 0,
      NOISSUES: 0,
      NODATA: 0
    };
    const severity_critical = this.getSeveritFromImage(image, "CRITICAL");
    const severity_high = this.getSeveritFromImage(image, "HIGH");
    const severity_medium = this.getSeveritFromImage(image, "MEDIUM");
    const severity_low = this.getSeveritFromImage(image, "LOW");

    counters.CRITICAL += severity_critical + severity_high; // these two combined
    counters.MEDIUM += severity_medium;
    counters.NOISSUES += severity_low;

    if (image.scanResult === "Failed") {
      counters.NODATA += 1;
      return { text: "No Data", score: -1 };
    }
    // no data
    if (severity_critical + severity_high + severity_medium + severity_low === 0) {
      counters.NOISSUES += 1;
      return { text: "No Data", score: -2 };
    }

    let results = [];
    if (severity_critical > 0) {
      results.push("<b class='severity-CRITICAL'>" + severity_critical + "</b> Critical");
      imageRowResult.score+= 1000;
    }
    if (severity_high > 0) {
      results.push("<b class='severity-HIGH'>" + severity_high + "</b> High");
      imageRowResult.score+= 100;
    }
    if (severity_medium > 0) {
      results.push("<b class='severity-MEDIUM'>" + severity_medium + "</b> Medium");
      imageRowResult.score+= 10;
    }
    if (severity_low > 0) {
      imageRowResult.score+= 1;
      results.push("<b class='severity-LOW'>" + severity_low + "</b> Low");
    }
    imageRowResult.text = results.join(", ");
    return imageRowResult
  }

  shortenImageName(name: string) {
    let chunks = name.split("/");
    return chunks[chunks.length - 1];
  }

  paintFilterName(text:string){    
    if (this.filter && this.filter.length > 1) {
      return text.replace(this.filter, '<span class="highlight">' + this.filter + '</span>');
    }
    return text;
  }

  renderList() {
    this.results = [];

    // scan images
    for (let i = 0; i < this.data.scans.length; i++) {
      const image = this.data.scans[i];
      for (let j = 0; j < image.attributes.length; j++) {
        const attribute = image.attributes[j];
        if (attribute.split(":")[0] === this.selectedAttribute) {
          // check if the image with the attribute value exists under the group
          const attributeValue = attribute.split(":")[1];
          const imageIndex = this.results.findIndex(o => o.title === attributeValue);
          if (imageIndex === -1) {
            this.results.push({
              // create new image under group
              title: attributeValue,
              images: [image],
              order: 255,
              counter: new NamespaceCounters()
            });
          } else {
            this.results[imageIndex].images.push(image); // add existing image under group
          }
        }
      }
    }

    // sort results by title
    this.results.sort((a, b) => a.title < b.title ? -1 : a.title > b.title ? 1 : 0);

    // sort result groups by severity
    for (let i = 0; i < this.results.length; i++) {
      let resultGroup = this.results[i];
      resultGroup.counter = new NamespaceCounters();

      if(this.filter && this.filter.length>1) { 
        if(
          resultGroup.title.indexOf(this.filter)===-1
        ){ continue }
        else{resultGroup.title = this.paintFilterName(resultGroup.title); }
      }

      for (let j = 0; j < resultGroup.images.length; j++) {
        const rowAnalysis = this.parseSeverities(resultGroup.images[j]);
        resultGroup.images[j].rowText = rowAnalysis.text;
        resultGroup.images[j].order = rowAnalysis.score;
        if (rowAnalysis.text === "No Issues") {
          resultGroup.counter.passing += 1;
        } else if (rowAnalysis.text === "No Data") {
          resultGroup.counter.nodata += 1;
        } else {
          resultGroup.counter.failing += 1;
        }
      }
    }

    // sort group by order
    this.results.sort((a, b) =>
      a.order < b.order ? -1 : a.order > b.order ? 1 : 0
    );

    // sort images by severity order
    for (let i = 0; i < this.results.length; i++) {
      const r = this.results[i];
      r.images.sort((a, b) => a.order > b.order ? -1 : a.order < b.order ? 1 : 0);
      for (let j = 0; j < r.images.length; j++) {
        r.images[j].shortImageName = this.paintFilterName(this.shortenImageName(r.images[j].image));
        if (r.images[j].rowText === "No Issues") {
          r.images[j].icon = "fas fa-check noissues-icon";
          r.images[j].tip = "Click to see detailed image analysis";
        } else if (r.images[j].rowText === "No Data") {
          r.images[j].icon = "fas fa-times nodata-icon";
          r.images[j].tip = "Failed to scan this image";
        } else {
          r.images[j].icon = "fas fa-exclamation-triangle warning-icon";
          r.images[j].tip = "Click to see detailed image analysis";
        }
        r.images[j].link = "/image-detail/" + encodeURIComponent(r.images[j].image);
      }

      if(this.filter && this.filter.length>1) { 
        r.images = r.images.filter(x=> x.shortImageName.includes(this.filter));
      }

    }

    
  }
}
