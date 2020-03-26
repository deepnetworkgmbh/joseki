<template>
  <div id="app" class="wrapper">
    <Navigation />
    <Breadcrumbs :date='date' :component='component' />
    <router-view @dateChanged='dateChanged' @componentChanged='componentChanged' />
    <div class="footer">
      ©2020
      <a href="http://www.deepnetwork.com" target="_blank">Deep Network GmbH</a>
    </div>
  </div>
</template>
<script lang="ts" src="./App.ts"></script>
<style lang="scss" src="./App.scss"></style>