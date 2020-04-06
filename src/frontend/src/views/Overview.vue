<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel" style="background-color:#eee;">
        <div class="p-5 pl-6 pt-4">
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
      <div class="w-2/4">
        <apexchart :options="getPieChartOptions()" :series="getPieChartSeries()"></apexchart>
      </div>
      <div class="w-1/4 border-l border-gray-300 top-right-panel" style="z-index:10;">
        <div class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-xs font-bold'>Scan History</div>
          <div style="width:100%;height:70px;">
            <apexchart height="70" :options="getAreaChartOptions()" :series="getAreaSeries()"></apexchart>
          </div>
        </div>
        <div class="m-3 mt-0">
          <div class='text-center text-xs font-bold border-b border-gray-500'>Last 5 scans</div>
          <table class="w-full text-xs p-4">
            <tbody>
              <tr v-for="(scan,i) in getShortHistory()" :key="`scan${i}`" @click="dayClicked(scan.recordedAt.split('T')[0])" :class='getHistoryClass(scan)'>
                <td>{{ scan.recordedAt | formatDate }}</td>
                <td class="w-1 text-right">{{scan.score}}%</td>
              </tr>
            </tbody>
          </table>
          <div class="text-center">
            <button class="btn mt-2" @click="goComponentHistory()">
              <span class="px-4"><span class="icon-more-vertical pr-1"></span>See Scan History</span>
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow">
      <div class="w-full flex flex-wrap pt-2 pl-1 justify-center">
        <InfComponent v-for="(c, i) in data.components" :key="`scan${i}`"
          :component="c.component"
          :score="c.current.score"
          :total="c.current.total"
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