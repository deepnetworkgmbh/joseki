<template>
  <div class="advanced-filter noselect"  @mouseleave="menuOpen=false">
    <div :class="menuOpen ? 'advanced-filter-menu-open': 'advanced-filter-menu-closed'" @mouseleave="menuOpen=false">
        <div class="advanced-filter-container" @click="menuOpen=!menuOpen">            
            <div v-for="(filter,i) in filterContainer.filters" :key="`filter${i}`">
                <div class="advanced-filter-tag">
                    <!-- <span v-if="menuOpen" class="icon-git-commit" style="float:left;margin:3px;background-color:#ccc;color:#fff;"></span> -->
                    <span style="padding:2px;background-color:#ccc;"> {{ filter.label }} </span> 
                    <!-- <span style="padding:2px;background-color:#ccc;color:#fff;">=</span> -->
                    <span v-for="(value,k) in filter.values" :key="`value-${i}-${k}`">
                      <span style="margin-right:1px;padding-left:2px;padding-right:2px;color:#444;">{{ value }}</span>
                      <span v-if="k<filter.values.length-1" style="color:#99a"> or </span>
                    </span>
                    <i v-if="menuOpen" class="icon-x-circle advanced-filter-delete-tag" @click="deleteFilter(i)" />
                </div>
                <div v-if="menuOpen" style="clear:both"></div>
            </div>
            <div v-if="filterContainer.filters.length===0">
              No active filter.
            </div>
        </div>
    </div>
  </div>
</template>
<script lang="ts" src="./AdvancedFilter.ts"></script>
<style scoped lang="scss" src="./AdvancedFilter.scss"></style>
