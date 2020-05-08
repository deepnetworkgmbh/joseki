<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:10px;flex-direction:column;">
      <div>
        <vue-markdown v-if="!edit" class="content">{{ documentation }}</vue-markdown>
        <!-- <textarea class="editor" v-if="edit" v-model="documentation"></textarea> -->
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./CheckDetail.ts"></script>
<style lang="scss" src="./CheckDetail.scss"></style>
