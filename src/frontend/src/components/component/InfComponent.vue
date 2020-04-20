<template>
  <div class="inf-component shadow">
    <div class="inf-component-left">
      <div class="inf-component-left-top">
        <div class='inf-component-icon'>
            <span :class="getComponentIcon(component.category)"></span>
        </div>
        <div class='inf-component-info'>
          <div class="inf-component-name">{{ component.name }}</div>
          <div class="inf-component-category">{{component.category}}</div>
        </div>
      </div>
      <div class="inf-component-histogram" :id="`bar${index}`">
        <apexchart height="50" :options="areaOptions" :series="areaSeries"></apexchart>
      </div> 
    </div>
    <div class="inf-component-right">
      <div class='inf-component-buttons'>
        <button class="gradient" @click="goComponentHistory()"><span class="icon-more-vertical"></span> History</button>
        <button class="gradient" @click="goComponentDetail()"><span class="icon-search"></span> Detail</button>
      </div>
      <div class="inf-component-pie">
        <apexchart height="70" :options="donutOptions" :series="donutSeries"></apexchart>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./InfComponent.ts"></script>
<style scoped lang="scss" src="./InfComponent.scss"></style>
