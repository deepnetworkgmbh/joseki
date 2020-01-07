<template>
  <div>
    <Spinner v-if="!loaded" class="centered" />
    <div v-show="loaded" class="segment summary shadow">
     <div class="overview-pie">
        <svg viewBox="0 0 36 36" class="circular-chart green">
          <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
          <path class="circle" stroke-dasharray="60, 100" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
          <text x="18" y="20.35" class="percentage">{{ data.cluster.score }}%</text>
        </svg>
      </div>
      <div class="info-tab">
        <h4 style="font-size:21px;line-height:28px;">Name: {{ data.cluster.name }}</h4>
        <h4 style="font-size:21px;line-height:28px;">Grade: {{ data.cluster.grade }}</h4>
        <h4 style="font-size:21px;line-height:28px;">Score: {{ data.cluster.score }}%</h4>
      </div>
      <div class="info-tab">
        <h4>Kubernetes Version: {{ data.cluster.version }}</h4>
        <h4>Nodes: {{ data.cluster.nodes }}</h4>
        <h4>Namespaces: {{ data.cluster.namespaces }}</h4>
        <h4>Pods: {{ data.cluster.Pods }}</h4>
      </div>
    </div>
    <div v-show="loaded" class="segment chart-group shadow">
      <div class="chart-group-container">
          <div class="chart-title">by categories</div>
          <div id="chart1" ref="chart1" class="chart"></div>
      </div>
      <div class="chart-group-container">
          <div class="chart-title">overall check result</div>
          <div id="chart2" ref="chart2" class="chart"></div>
      </div>
      <div class="chart-group-container">
          <div class="chart-title">by namespace</div>
          <div id="chart3" ref="chart3" class="chart"></div>
      </div>
    </div>
    <div v-show="loaded" class="data-table shadow" ref="table">
    </div>
  </div>
</template>

<script lang="ts" src="./Home.ts"></script>
<style lang="scss" src="./Home.scss"></style>
