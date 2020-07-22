<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel" style="background-color:#eee;">
        <div class="p-5 pl-6">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ selectedDate | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.overall.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.overall.current.score) }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Clusters</h5>
            <h1 class="info">{{ getClusters() }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Subscriptions</h5>
            <h1 class="info">{{ getSubscriptions() }}</h1>
          </div>
        </div>
      </div>
      <div class="w-2/4" style="height:420px;">
        <apexchart v-if="!noScanHistory" :options="getPieChartOptions()" :series="getPieChartSeries()"></apexchart>
        <div v-if="noScanHistory" style="font-size:10px;color:#444;padding:10px;"> 
          <h1 style="font-size:16px;">Where is my Scan data?</h1> 
          If this is a fresh install, please give it some time until scanners kick off.<br>
          <br>
          If not, one of the following happened;<br>       
          - <b>scanner</b> failed to perform the audit itself. To check it, take a look at scanner logs;<br>
          - <b>backend</b> failed to parse scan results. In such a case, dive into <b>backend</b> logs for clues.<br>
          <br>
          Also, you can check raw audit results at blob storage. <br>
          <b>meta</b> file in the audit folder describes the result of the audit: _failed_, _succeeded_, and the scan related metadata.
          <br>
          <br>
          This page will automatically check any scan result every 10 seconds.
        </div>
      </div>
      <div class="w-1/4 border-l border-gray-300 top-right-panel" style="z-index:10;">
        <div v-if="!noScanHistory" class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-xs font-bold'>Scan History</div>
          <div style="width:100%;height:70px;">
            <apexchart height="70" :options="getAreaChartOptions()" :series="getAreaSeries()"></apexchart>
          </div>
        </div>
        <div v-if="!noScanHistory" class="m-3 mt-0">
          <div class='text-center text-xs font-bold border-b border-gray-500'>Last 7 scans</div>
          <table class="w-full text-xs p-4">
            <tbody>
              <tr v-for="(scan,i) in getShortHistory()" :key="`scan${i}`" @click="dayClicked(scan.recordedAt.split('T')[0])" :class='getHistoryClass(scan)'>
                <td>{{ scan.recordedAt | formatDate }}</td>
                <td class="w-1 text-right">{{scan.score}}%</td>
              </tr>
            </tbody>
          </table>
          <div class="text-center">
            <button class="btn mt-2 gradient" @click="goComponentHistory()">
              <span class="px-4"><span class="icon-more-vertical pr-1"></span>See Scan History</span>
            </button>
          </div>
        </div>
        <div v-if="noScanHistory" style="padding:10px;font-size:9px;">
          No scan history yet.<br>          
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow">
      <div class="w-full flex flex-wrap pt-2 pl-1 justify-center">
        <InfComponent v-for="(c, i) in data.components" :key="`scan${i}`"
          :component="c.component"
          :score="c.current.score"
          :date="selectedDate"
          :index="i"
          :scoreHistory="data.components[i].scoreHistory"
          :summary="data.components[i].current"
          @dateChanged="dayClicked"></InfComponent>        
      </div>
    </div>
    
  </div>
</template>

<script lang="ts" src="./Overview.ts"></script>
<style lang="scss" src="./Overview.scss"></style>
