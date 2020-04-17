<template>
  <div class="paginator">
      <button v-for="(btn,i) in pageButtons" :key="`btn${i}`" :class="getButtonClass(btn)" @click="changePageIndex(btn -1)">
          {{ btn }}
      </button>
      <div style="font-size:9px;color:#444;float:right;margin:5px;">({{ totalRows }} {{ totalRows >0 ? "records": "record"}})</div>
  </div>
</template>
<script lang="ts" src="./Paginator.ts"></script>
<style scoped lang="scss" src="./Paginator.scss"></style>
