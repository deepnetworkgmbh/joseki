<template>
  <div class="result-filter">
    <div v-if="menuOpen" class="result-filter-menu">
        <div class="result-filter-severities">
            <div class="result-filter-severities-title">Severities</div>       
            <label class="noselect"><input type="checkbox" v-model="severities.nodata"> No Data</label>
            <label class="noselect"><input type="checkbox" v-model="severities.failed"> Failed</label>
            <label class="noselect"><input type="checkbox" v-model="severities.warning"> Warning</label>
            <label class="noselect"><input type="checkbox" v-model="severities.success"> Success</label>
        </div>
    </div>
    <button class='btn-mini' @click='menuOpen=!menuOpen' ><i class="icon-filter" /></button>
  </div>
</template>
<script lang="ts" src="./ResultFilter.ts"></script>
<style scoped lang="scss" src="./ResultFilter.scss"></style>
