<template>
  <div id="app">
    <Navigation />
    <div class="container">
      <router-view />
    </div>
    <div class="footer">
      ©2019
      <a href="http://www.deepnetwork.com" target="_blank">Deep Network GmbH</a>
    </div>
  </div>
</template>
<script lang="ts" src="./App.ts"></script>
<style scoped lang="scss" src="./App.scss"></style>
