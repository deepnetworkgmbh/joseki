<template>
  <div id="nav" class='mb-1'>
    <div class="logo">
      <router-link to="/">
      <font-awesome-icon icon="shield-alt" />
      Joseki</router-link>
    </div>
    <div class="links">
      <router-link to="/" :active-class="returnClusterOverviewActive()">Cluster Overview</router-link>
      <router-link to="/image-overview" :active-class="returnImageOverviewActive()">Image Overview</router-link>
      <router-link to="/docs">Documentation</router-link>
    </div>
  </div>
</template>
<script lang="ts" src="./Navigation.ts"></script>
<style scoped lang="scss" src="./Navigation.scss"></style>
