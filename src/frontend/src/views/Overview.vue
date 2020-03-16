<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0;">
      <div
        class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center"
        style="overflow:hidden;">
        <div class="status-icon">
          <i :class="getScoreIconClass(data.overall.current.score)"></i>
        </div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">
            {{ selectedDate | formatDate }}
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.overall.current.score }}%</div>
          </div>
          <div class="flex flex-row big-text xl:text-2xl lg:text-xl">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.overall.current.score) }}</div>
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
        <div class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-xs font-bold'>Scan History</div>
          <div id="overall_bar" style="width:100%;"></div>
        </div>
        <div class="m-3 mt-0">
          <div class='text-center text-xs font-bold border-b border-gray-500'>Last 5 scans</div>
          <table class="w-full text-xs p-4">
            <tbody>
              <tr v-for="(scan,i) in shortHistory" :key="`scan${i}`">
                <td>{{ scan.recordedAt | formatDate }}</td>
                <td class="w-1">{{scan.score}}%</td>
                <td class="w-1" v-html="getArrowHtml(i)"></td>
              </tr>
            </tbody>
          </table>
          <div class="text-right">
            <button class="btn mt-2" @click="goComponentHistory()">
              <span class="px-4"><i class="fas fa-history pr-2"></i>See Scan History</span>
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow">
      <div class="w-full flex flex-wrap pt-2 pl-1 justify-center">
        <InfComponent v-for="(c, i) in data.components" :key="`scan${i}`"
          :component="c.component"
          :sections="c.sections"
          :score="c.current.score"
          :total="c.current.total"
          :date="selectedDate"
          :index="i"
        ></InfComponent>        
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