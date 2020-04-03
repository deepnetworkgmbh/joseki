<template>
  <div class="inf-component-diff shadow">
    <div class="inf-component-diff-left">
      <div class="inf-component-diff-left-top">
        <div class='inf-component-diff-icon'>
          <span :class="getComponentIcon()"></span>
        </div>
        <div class='inf-component-diff-info'>
          <div class="inf-component-diff-name">{{ component.name }}</div>
          <div class="inf-component-diff-category">{{component.category}}</div>
        </div>
      </div>
      <div class="inf-component-diff-histogram">
          <apexchart height="50" :options="areaOptions" :series="areaSeries"></apexchart>
      </div>
    </div>
    <div class="inf-component-diff-right">
      <div class='inf-component-diff-buttons'>
        <button @click="goComponentHistory(component)"><span class="icon-more-vertical"></span> History</button>
        <button v-if="notLoaded" :disabled="notLoaded">
         <span class='text-center' data-balloon-length="xlarge" data-balloon-pos="left" 
            aria-label="This component has no overview on one of the dates, thus cannot be compared.">
            Compare<i class="icon-help-circle tip-icon" style="font-size:9px;"></i>
         </span></button>
        <button v-if="!notLoaded" @click="goComponentDiff(component)" :disabled="notLoaded">
          <i class="icon-copy pr-1"></i>Differences</button>
      </div>
      <div class="inf-component-diff-pies">
        <div>
          <div class="inf-component-diff-pie">
              <apexchart height="60" :options="donutOptions1" :series="donutSeries1"></apexchart>
          </div>
        </div>
        <div class="inf-component-diff-arrow">►</div>
        <div>
          <div class="inf-component-diff-pie">
              <apexchart height="60" :options="donutOptions2" :series="donutSeries2"></apexchart>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./DiffComponent.ts"></script>
<style scoped lang="scss" src="./DiffComponent.scss"></style>
