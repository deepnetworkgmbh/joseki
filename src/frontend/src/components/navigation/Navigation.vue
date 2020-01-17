<template>
  <div id="nav">
    <div class="logo">
      <router-link to="/">
      <font-awesome-icon icon="shield-alt" />
      KubeGaard</router-link>
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
