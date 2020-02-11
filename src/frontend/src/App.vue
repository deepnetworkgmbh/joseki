<template>
  <div id="app" :class="getWrapperClass()">
    <Navigation />
    <router-view @sideWindowOpened='toggleSideWindow(true)' @sideWindowClosed='toggleSideWindow(false)' />
    <div class="footer">
      ©2020
      <a href="http://www.deepnetwork.com" target="_blank">Deep Network GmbH</a>
    </div>
  </div>
</template>
<script lang="ts" src="./App.ts"></script>
<style lang="scss" src="./App.scss"></style>