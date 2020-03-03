<template>
  <div class="inf-component shadow">
    <div class="inf-component-left">
      <div class="inf-component-left-top">
        <div class='inf-component-icon'>
          <img v-show="component.category==='Azure Subscription'" src='@/assets/azure.png'>
          <img v-show="component.category==='Kubernetes'" src='@/assets/kubernetes.png'>
        </div>
        <div class='inf-component-info'>
          <div class="inf-component-name">{{ component.name }}</div>
          <div class="inf-component-category">{{component.category}}</div>
        </div>
      </div>
      <div class="inf-component-histogram" :id="`bar${index}`"></div>
    </div>
    <div class="inf-component-right">
      <div v-show="showButtons" class='inf-component-buttons'>
        <button @click="goComponentHistory(component)">History</button>
        <button @click="goComponentDetail(component)">Detail</button>
      </div>
      <div class="inf-component-score">{{score}}%</div>
      <div class="inf-component-pie">
        <vc-donut
          :sections="sections"
          :size="60"
          unit="px"
          :total="total"
          :thickness="25"
        ></vc-donut>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./InfComponent.ts"></script>
<style scoped lang="scss" src="./InfComponent.scss"></style>
