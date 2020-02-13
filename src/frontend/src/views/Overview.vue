<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div
        class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center"
        style="overflow:hidden;"
      >
        <div class="status-icon">
          <i :class="getScoreIconClass()"></i>
        </div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">
            {{ date | formatDate }}
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ score }}%</div>
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ grade }}</div>
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Clusters:</div>
            <div class="w-5/12 font-hairline text-left">{{ getClusters() }}</div>
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Subscriptions:</div>
            <div class="w-5/12 font-hairline text-left">{{ getSubscriptions() }}</div>
          </div>
        </div>
      </div>
      <div class="w-2/4">
        <div id="overall_pie" class="w-auto" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300" style="z-index:10;">
        <div id="overall_bar" class="w-auto border-b border-gray-500 p-2 ml-1 mb-3"></div>
        <div class="m-3 mt-0">
          <table class="border border-gray-200 w-full text-xs p-4">
            <thead>
              <tr class="bg-gray-500 text-white">
                <th colspan="3">Last Scans</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(scan,i) in shortHistory" :key="`scan${i}`">
                <td>{{ scan.recordedAt | formatDate }}</td>
                <td class="w-1">{{scan.score}}%</td>
                <td class="w-1" v-html="getArrowHtml(i)"></td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <td colspan="3" class="bg-gray-500 text-center">
                  <button class="btn" @click="panelOpen=!panelOpen">See All</button>
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
    <!-- <div v-if="loaded" class="segment shadow">
        <div class="flex flex-row w-full">
            <div class="w-1/2">            
                <div v-if='viewMode === 0'>List View</div>
                <div v-if='viewMode === 1'>Detailed View</div>
            </div>
            <div class="w-1/2 text-right">            
                <button :class="getViewModeClass(0)" @click="setViewMode(0)"><i class="fa fa-list"></i></button>
                <button :class="getViewModeClass(1)" @click="setViewMode(1)"><i class="fas fa-th"></i></button>
            </div>
        </div>        
    </div>-->
    <div v-if="loaded" class="segment shadow">
      <div v-if="viewMode === 0" class="w-full">
        <div
          v-for="(scan, i) in scans"
          :key="`scan${i}`"
          class="w-full scan-list-item"
        >{{ scan.target }}</div>
      </div>
      <div v-if="viewMode === 1" class="w-full flex flex-wrap pt-2 pl-1">
        <div v-for="(c, i) in data.components" :key="`scan${i}`" class="scan-detailed-item flex flex-row shadow">
          <div class="w-full p-2 text-lg pt-0 flex flex-col">
            <div class="text-sm">{{ c.component.name }}</div>
            <div class="text-xs text-gray-600">{{c.component.category}}</div>
            <div style="height:50px;width:130px;" :id="`bar${i}`"></div>
          </div>
          <div class="p-2" style="width: 100px;">
            <div style="position:relative;font-size:18px;z-index:1;left:19px;top:23px;">{{c.current.score}}%</div>
            <div style="position:relative;top:-25px;z-index:0;">
              <vc-donut
                :sections="c.sections"
                :size="70"
                unit="px"
                :total="c.current.total"
                :thickness="25"
              ></vc-donut>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div :class="getPanelClass()" style="z-index:90;">
      <div v-if="panelOpen" class="p-2">
        <button class="btn m-1" @click="panelOpen=false" style="float:right;border:none;">X</button>
        <h1 class="-mt-1">Select Scans to Compare</h1>
        <h1 class="mt-1 font-bold">for Overall</h1>
        <div class="scan-list">
          <table style="width:100%">
            <tr v-for="(scan,i) in data.overall.scoreHistory" :key="`scan${i}1`">
              <td class="w-1">
               <input type="checkbox" class='chk' :id="`scan${i}1`" :value="`${scan.recordedAt}`" v-model="checkedScans" 
               :disabled="checkDisabled(i, `${scan.recordedAt}`)">
              </td>
              <td class='text-sm'>{{ scan.recordedAt | formatDate }}</td>
              <td class='text-sm'>{{ scan.id }}</td>
              <td class="w-1" v-html="getArrowHtml(i)"></td>
              <td class="w-1 text-sm">{{scan.score}}%</td>
            </tr>
          </table>
        </div>
        <div class='panel-button-container'>
          <div class='text-right m-2'>
            <button class="btn" @click="CompareScans()" :disabled='canCompare()'>Compare</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./Overview.ts"></script>
<style lang="scss" src="./Overview.scss"></style>