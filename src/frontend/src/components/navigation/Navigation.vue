<template>
  <div id="nav" class='flex flex-row p-2'>
    <div class="logo">
      <router-link to="/">
        <span class="icon-shield"></span>Joseki
      </router-link>
    </div>
    <!-- <div class="links flex-grow justify-end text-right pt-1">
      <router-link to="/" :active-class="returnClusterOverviewActive()">Cluster Overview</router-link>
      <router-link to="/image-overview" :active-class="returnImageOverviewActive()">Image Overview</router-link>
      <router-link to="/docs">Documentation</router-link>
    </div> -->
  </div>
</template>
<script lang="ts" src="./Navigation.ts"></script>
<style scoped lang="scss" src="./Navigation.scss"></style>
