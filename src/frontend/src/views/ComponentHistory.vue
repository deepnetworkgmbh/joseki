<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">  
      <div style="display:flex;flex-direction:column;width:100%">
        <div class='subscription-history-header'>
          <h1>{{ component.category }} : <b>{{ decodeURIComponent(component.id) }}</b></h1>
          <div style='font-size:15px;color:#666;'>Select Scans to Compare</div>
        </div>
        <div class='subscription-history-list'>
          <table style="width:100%">
            <thead style="border-bottom:solid 1px #aaa;">
                <tr>
                    <td colspan="2" class='w-7/12 '>Scan Date</td>
                    <td colspan="2" class='w-1/12 pl-5'>No Data</td>
                    <td colspan="2" class='w-1/12 pl-5'>Warning</td>
                    <td colspan="2" class='w-1/12 pl-5'>Failed</td>
                    <td colspan="2" class='w-1/12 pl-5'>Passed</td>
                    <td colspan="2" class='w-1/12 pl-5'>Score</td>
                </tr>
            </thead>
            <tr v-for="(scan,i) in data" :key="`scan${i}1`">
              <td colspan="2">
               <label :for="`scan${i}1`">
               <input type="checkbox" class='chk' :id="`scan${i}1`" :value="`${scan.date}`" v-model="checkedScans" 
               :disabled="checkDisabled(i, `${scan.date}`)">
               <span class="pl-2">{{ scan.date | formatDate }}</span>
               </label>
              </td>
              <td class='w-1 text-xs text-right' v-html="getErrorArrowHtml('nodata', i)"></td>
              <td class='text-sm text-left'>{{ scan.current.nodata || '0' }}</td>
              <td class='w-1 text-xs text-right' v-html="getErrorArrowHtml('warning', i)"></td>
              <td class='text-sm text-left'>{{ scan.current.warning || '0' }}</td>
              <td class='w-1 text-xs text-right' v-html="getErrorArrowHtml('failed', i)"></td>
              <td class='text-sm text-left'>{{ scan.current.failed || '0' }}</td>
              <td class='w-1 text-xs text-right' v-html="getErrorArrowHtml('passed', i, true)"></td>
              <td class='text-sm text-left'>{{ scan.current.passed || '0' }}</td>
              <td class="w-1 text-xs text-right" v-html="getArrowHtml(i)"></td>
              <td class="text-sm text-left">{{scan.current.score}}%</td>
            </tr>           
          </table> 
        </div>
        <div class="subscription-history-buttons">
            <button class="btn" @click="GoBack()">
              <i class="fas fa-chevron-left pr-2"></i>Back
            </button>
            <button class="btn" @click="CompareScans()" :disabled='canCompare()'>
              <i class="fas fa-not-equal pr-2"></i>Compare
            </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentHistory.ts"></script>
<style lang="scss" src="./ComponentHistory.scss"></style>