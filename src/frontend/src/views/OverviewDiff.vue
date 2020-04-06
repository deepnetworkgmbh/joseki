<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel" style="overflow:hidden;background-color:#eee;">
        <div class="status-text p-5 pl-4 pt-4">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ date | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.summary1.overall.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.summary1.overall.current.score) }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Clusters</h5>
            <h1 class="info">{{ getClusters1() }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Subscriptions</h5>
            <h1 class="info">{{ getSubscriptions1() }}</h1>
          </div>
        </div>
      </div>
      <div class="w-1/4 pt-10">
        <apexchart :options="getPieChartOptions1()" :series="getPieChartSeries1()"></apexchart>
      </div>
      <div class="w-1/4 pt-10 top-seperator">
        <apexchart :options="getPieChartOptions2()" :series="getPieChartSeries2()"></apexchart>
      </div>
      <!-- <div class="w-2/4">
        <apexchart :options="getDiffAreaChartOptions()" :series="getDiffAreaSeries()"></apexchart>
      </div> -->
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center top-right-panel" style="overflow:hidden;background-color:#eee;">
        <div class="status-text p-5 pl-4 pt-4">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ date2 | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.summary2.overall.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.summary2.overall.current.score) }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Clusters</h5>
            <h1 class="info">{{ getClusters2() }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Subscriptions</h5>
            <h1 class="info">{{ getSubscriptions2() }}</h1>
          </div>
        </div>
      </div>       
    </div>
    <div v-if="loaded" class="segment shadow">
      <div class="w-full flex flex-wrap pt-2 pl-1 justify-center">
        <DiffComponent v-for="(c, i) in data.compositeComponents" :key="`scan${i}`"
          :component="c.component"
          :date="date"
          :date2="date2"
          :index="i"
          :scoreHistory="c.scoreHistory"
          :summary1="getCurrentFromSummary(data.summary1, c.component.id)"
          :summary2="getCurrentFromSummary(data.summary2, c.component.id)"
        ></DiffComponent>        
      </div>
    </div>    
  </div>
</template>

<script lang="ts" src="./OverviewDiff.ts"></script>
<style lang="scss" src="./OverviewDiff.scss"></style>