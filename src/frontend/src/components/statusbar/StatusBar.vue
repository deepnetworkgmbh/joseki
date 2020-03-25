<template>
  <span class='text-center' data-balloon-length="xlarge" data-balloon-pos="left" :aria-label="counters.getString()" style="top:17px;">
    <div class="statusbar shadow">
      <div v-if="counters.noData>0" :class="getClass(0)" :style="{ width: getWidth(0) }">{{ counters.noData }}</div>
      <div v-if="counters.failed>0" :class="getClass(1)" :style="{ width:getWidth(1) }">{{ counters.failed }}</div>
      <div v-if="counters.warning>0" :class="getClass(2)" :style="{ width:getWidth(2) }">{{ counters.warning }}</div>
      <div v-if="counters.passed>0" :class="getClass(3)" :style="{ width:getWidth(3) }">{{ counters.passed }}</div>
    </div>
  </span>
</template>
<script lang="ts" src="./StatusBar.ts"></script>
<style scoped lang="scss" src="./StatusBar.scss"></style>
