<template>
  <div id="nav">
    <div class="logo">
      <font-awesome-icon icon="shield-alt" />
      KubeGaard
    </div>
    <div class="links">
      <router-link to="/">Cluster Overview</router-link>
      <router-link to="/image-overview">Image Overview</router-link>
      <router-link to="/docs">Documentation</router-link>
    </div>
  </div>
</template>
<script lang="ts" src="./Navigation.ts"></script>
<style scoped lang="scss" src="./Navigation.scss"></style>
