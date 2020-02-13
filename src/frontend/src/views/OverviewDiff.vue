<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.overall1.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.overall1.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.overall1.current.score) }}</div>
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
      <div class="w-1/4" style="overflow:hidden;border-left:dashed 2px #aaa">
        <div id="overall_pie2" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.overall2.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date2 | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.overall2.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.overall2.current.score) }}</div>
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
      <div class="w-full flex flex-wrap pt-2 pl-1">
        <div v-for="(c, i) in data.components1" :key="`scan${i}`" class="scan-detailed-diff-item flex flex-row shadow">
          <div class="w-full p-2 text-lg pt-0 flex flex-col">
            <div class="text-sm">{{ c.component.name }}</div>
            <div class="text-xs text-gray-600">{{c.component.category}}</div>
            <div style="height:50px;width:130px;" :id="`bar${i}`"></div>
          </div>
          <div class="p-2 pl-0" style="width: 100px;">
            <div style="position:relative;font-size:18px;z-index:1;left:19px;top:23px;">{{c.current.score}}%</div>
            <div style="position:relative;top:-25px;z-index:0;">
              <vc-donut :sections="c.sections" :size="70" unit="px" :total="c.current.total" :thickness="25"></vc-donut>
            </div>
          </div>
          <div style="width:10px;padding:0;margin:0;padding-top:30px;color:green;">⮀</div>
          <div class="p-2" style="width: 100px;">
            <div style="position:relative;font-size:18px;z-index:1;left:19px;top:23px;">{{data.components2[i].current.score}}%</div>
            <div style="position:relative;top:-25px;z-index:0;">
              <vc-donut :sections="data.components2[i].sections" :size="70" unit="px" :total="data.components2[i].current.total" :thickness="25"></vc-donut>
            </div>
          </div>
        </div>
      </div>
    </div>
    
  </div>
</template>

<script lang="ts" src="./OverviewDiff.ts"></script>
<style lang="scss" src="./OverviewDiff.scss"></style>