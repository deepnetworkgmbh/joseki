<template>
  <div class="status-bar">
    <div class="status">
      <div class="failing">
        <div class="nodata" :style="{ width: noDataWidth() }">
          <div class="passing" :style="{ width: passingWidth() }"></div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./StatusBar.ts"></script>
<style scoped lang="scss" src="./StatusBar.scss"></style>
