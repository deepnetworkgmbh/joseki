<template>
  <div class="inf-component-diff shadow">
    <div class="inf-component-diff-left">
      <div class="inf-component-diff-left-top">
        <div class='inf-component-diff-icon'>
          <img v-show="component.category==='Azure Subscription'" src='@/assets/azure.png'>
          <img v-show="component.category==='Kubernetes'" src='@/assets/kubernetes.png'>
        </div>
        <div class='inf-component-diff-info'>
          <div class="inf-component-diff-name">{{ component.name }}</div>
          <div class="inf-component-diff-category">{{component.category}}</div>
        </div>
      </div>
      <div class="inf-component-diff-histogram" :id="`bar${index}`"></div>
    </div>
    <div class="inf-component-diff-right">
      <div class='inf-component-diff-buttons'>
        <button @click="goComponentHistory(component)">History</button>
        <button v-if="notLoaded" :disabled="notLoaded">
         <span class='text-center' data-balloon-length="xlarge" data-balloon-pos="left" 
            aria-label="This component has no overview on one of the dates, thus cannot be compared.">Compare<i class="far fa-question-circle tip-icon"></i></span></button>
        <button v-if="!notLoaded" @click="goComponentDiff(component)" :disabled="notLoaded">Differences</button>
      </div>
      <div class="inf-component-diff-pies">
        <div>
          <div class="inf-component-diff-score">{{score}}%</div>
          <div class="inf-component-diff-pie">
              <vc-donut :sections="sections" :size="60" unit="px" :total="total" :thickness="25"></vc-donut>
          </div>
        </div>
        <div class="inf-component-diff-arrow">►</div>
        <div>
          <div class="inf-component-diff-score">{{score2}}%</div>
          <div class="inf-component-diff-pie">
              <vc-donut :sections="sections2" :size="60" unit="px" :total="total2" :thickness="25"></vc-donut>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./DiffComponent.ts"></script>
<style scoped lang="scss" src="./DiffComponent.scss"></style>
