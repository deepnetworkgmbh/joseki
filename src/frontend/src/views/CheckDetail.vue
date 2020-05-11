<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:10px;flex-direction:column;">
      <div>
        <div class="content" v-html="documentation"></div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./CheckDetail.ts"></script>
<style lang="scss" src="./CheckDetail.scss"></style>
