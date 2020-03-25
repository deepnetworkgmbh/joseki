<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel" style="overflow:hidden;background-color:#eee;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary1.overall.current.score)"></i></div>
         <div class="status-text p-5 pl-10 pt-16">
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
      <div class="w-1/4">
        <div id="overall_pie1" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4" style="overflow:hidden;border-left:dashed 2px #eee">
        <div id="overall_pie2" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center top-right-panel" style="overflow:hidden;background-color:#eee;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary2.overall.current.score)"></i></div>
         <div class="status-text p-5 pl-10 pt-16">
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
        <DiffComponent v-for="(c, i) in data.summary1.components" :key="`scan${i}`"
          :component="c.component"
          :sections="c.sections"
          :score="c.current.score"
          :total="c.current.total"
          :notLoaded="c.notLoaded"
          :date="date"
          :index="i"
          :sections2="data.summary2.components[i].sections"
          :score2="data.summary2.components[i].current.score"
          :total2="data.summary2.components[i].current.total"          
          :date2="date2"
        ></DiffComponent>        
      </div>
    </div>    
  </div>
</template>

<script lang="ts" src="./OverviewDiff.ts"></script>
<style lang="scss" src="./OverviewDiff.scss"></style>