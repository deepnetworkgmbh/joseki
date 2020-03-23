<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary1.overall.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.summary1.overall.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.summary1.overall.current.score) }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Clusters:</div>
            <div class="w-5/12 font-hairline text-left">{{ getClusters1() }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Subscriptions:</div>
            <div class="w-5/12 font-hairline text-left">{{ getSubscriptions1() }}</div>
          </div>
        </div>
      </div>
      <div class="w-1/4">
        <div id="overall_pie1" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4" style="overflow:hidden;border-left:dashed 2px #eee">
        <div id="overall_pie2" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary2.overall.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date2 | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.summary2.overall.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.summary2.overall.current.score) }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Clusters:</div>
            <div class="w-5/12 font-hairline text-left">{{ getClusters2() }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Subscriptions:</div>
            <div class="w-5/12 font-hairline text-left">{{ getSubscriptions2() }}</div>
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