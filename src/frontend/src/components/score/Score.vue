<template>
  <span :class="scoreClass">
      {{label}} <strong>{{score}}%</strong>
  </span>
</template>
<script lang="ts" src="./Score.ts"></script>
<style scoped lang="scss" src="./Score.scss"></style>