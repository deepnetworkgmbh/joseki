<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-show="loaded" class="segment shadow">
      <div class="w-1/5">
        <svg viewBox="0 0 36 36" class="circular-chart green">
          <path
            class="circle-bg"
            d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          />
          <path
            class="circle"
            stroke-dasharray="60 100"
            d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          />
          <text x="18" y="20.35" class="percentage">{{ data.cluster.score }}%</text>
        </svg>
      </div>
      <div class="w-2/5 flex flex-col justify-center">
        <h4 class="text-lg">Name: {{ data.cluster.name }}</h4>
        <h4 class="text-lg">Grade: {{ data.cluster.grade }}</h4>
        <h4 class="text-lg">Score: {{ data.cluster.score }}%</h4>
      </div>
      <div class="w-2/5 flex flex-col justify-center">
        <h4>Kubernetes Version: {{ data.cluster.version }}</h4>
        <h4>Nodes: {{ data.cluster.nodes }}</h4>
        <h4>Namespaces: {{ data.cluster.namespaces }}</h4>
        <h4>Pods: {{ data.cluster.pods }}</h4>
      </div>
    </div>
    <div v-show="loaded" class="segment shadow">
      <div class="w-1/3 flex-row">
        <div class="text-lg text-center">by categories</div>
        <div id="chart1" ref="chart1" class="w-auto"></div>
      </div>
      <div class="w-1/3 flex-row">
        <div class="text-lg text-center">overall check result</div>
        <div id="chart2" ref="chart2" class="w-auto"></div>
      </div>
      <div class="w-1/3 flex-row">
        <div class="text-lg text-center">by namespace</div>
        <div id="chart3" ref="chart3" class="w-auto"></div>
      </div>
    </div>
    <div v-show="loaded" class="shadow" ref="table"></div>
  </div>
</template>

<script lang="ts" src="./ClusterOverview.ts"></script>
<style lang="scss" src="./ClusterOverview.scss"></style>