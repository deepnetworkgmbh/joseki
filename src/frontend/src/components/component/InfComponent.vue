<template>
  <div class="inf-component shadow">
    <div class="inf-component-left">
      <div class="inf-component-left-top">
        <div class='inf-component-icon'>
           <span :class="getComponentIcon()"></span>
        </div>
        <div class='inf-component-info'>
          <div class="inf-component-name">{{ component.name }}</div>
          <div class="inf-component-category">{{component.category}}</div>
        </div>
      </div>
      <div class="inf-component-histogram" :id="`bar${index}`"></div>
    </div>
    <div class="inf-component-right">
      <div class='inf-component-buttons'>
        <button @click="goComponentHistory(component)"><span class="icon-more-vertical"></span> History</button>
        <button @click="goComponentDetail(component)"><span class="icon-search"></span> Detail</button>
      </div>
      <div class="inf-component-pie">
        <vc-donut :sections="sections" :size="60" unit="px" :auto-adjust-text-size="false" :text="`${this.score}%`" :total="total" :thickness="25"></vc-donut>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./InfComponent.ts"></script>
<style scoped lang="scss" src="./InfComponent.scss"></style>
