<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">  
      <div style="display:flex;flex-direction:column;width:100%">
        <div class='subscription-history-header'>
          <h1>Select Scans to Compare</h1>
          <h1 class="font-bold">for {{ component.category }}</h1>
        </div>
        <div class='subscription-history-list'>
          <table style="width:100%">
            <thead style="border-bottom:solid 1px #aaa;">
                <tr>
                    <td colspan="2">Scan Date</td>
                    <td>No Data</td>
                    <td>Warning</td>
                    <td>Failed</td>
                    <td>Passed</td>
                    <td colspan="2">Score</td>
                </tr>
            </thead>
            <tr v-for="(scan,i) in data" :key="`scan${i}1`">
              <td class="w-1">
               <input type="checkbox" class='chk' :id="`scan${i}1`" :value="`${scan.date}`" v-model="checkedScans" 
               :disabled="checkDisabled(i, `${scan.date}`)">
              </td>
              <td class='text-sm'>{{ scan.date | formatDate }}</td>
              <td class='text-sm'>1:{{ scan.current.nodata || '0' }}</td>
              <td class='text-sm'>2:{{ scan.current.warning || '0' }}</td>
              <td class='text-sm'>3:{{ scan.current.failed || '0' }}</td>
              <td class='text-sm'>4:{{ scan.current.passed || '0' }}</td>
              <td class="w-1" v-html="getArrowHtml(i)"></td>
              <td class="w-1 text-sm text-left">{{scan.current.score}}%</td>
            </tr>           
          </table> 
        </div>
        <div class="subscription-history-buttons">
            <button class="btn" @click="GoBack()">Back</button>
            <button class="btn" @click="CompareScans()" :disabled='canCompare()'>Compare</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentHistory.ts"></script>
<style lang="scss" src="./ComponentHistory.scss"></style>