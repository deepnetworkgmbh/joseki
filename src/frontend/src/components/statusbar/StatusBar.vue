<template>
  <div class="status-bar">
    <div class="status" :style="{ height: height}">
      <div class="failing" :style="{  height: height}">
        <div class="nodata" :style="{ width: noDataWidth() , height: height}">
          <div class="passing" :style="{ width: passingWidth()  , height: height}"></div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./StatusBar.ts"></script>
<style scoped lang="scss" src="./StatusBar.scss"></style>
