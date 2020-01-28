<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon">
            <i :class="getScoreIconClass()"></i>
        </div>
        <div class="status-text">
            <div class="flex flex-row big-text xl:text-3xl lg:text-2xl">
            <div class="w-9/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-3/12 font-hairline text-left">72%</div>
            </div>
            <div class="flex flex-row big-text xl:text-3xl lg:text-2xl">
            <div class="w-9/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-3/12 font-hairline text-left">B+</div>
            </div>
            <div class="flex flex-row big-text xl:text-3xl lg:text-2xl">
            <div class="w-9/12 font-thin text-right mr-1 text-gray-600">Clusters:</div>
            <div class="w-3/12 font-hairline text-left">4</div>
            </div>
            <div class="flex flex-row big-text xl:text-3xl lg:text-2xl">
            <div class="w-9/12 font-thin text-right mr-1 text-gray-600">Subscriptions:</div>
            <div class="w-3/12 font-hairline text-left">2</div>
            </div>
        </div>
      </div>
      <div class="w-2/4">
        <div ref="chart2" class="w-auto"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300">
        <div ref="chart3" class="w-auto p-2 ml-1"></div>
        <div class="m-3 mt-0">
            <table class="border border-gray-200 w-full text-xs p-4">
                <thead>
                    <tr class='bg-gray-500 text-white'>
                        <th>Latest Scans</th>
                        <th colspan='2'>Success</th>
                    </tr>
                </thead>
                <tbody>
                <tr>
                    <td>28.01.2020 00:16:12</td>
                    <td class="w-1">76%</td>
                    <td class="w-1"><i class="fas fa-arrow-up" style="color:green;"></i></td>
                </tr>
                <tr class="bg-gray-200">
                    <td>27.01.2020 00:16:11</td>
                    <td>74%</td>
                    <td><i class="fas fa-arrow-up" style="color:green;"></i></td>
                </tr>
                <tr>
                    <td>26.01.2020 00:16:14</td>
                    <td>72%</td>
                    <td><i class="fas fa-arrow-down" style="color:red;"></i></td>
                </tr>
                <tr class="bg-gray-200">
                    <td>25.01.2020 00:16:12</td>
                    <td>76%</td>
                    <td><i class="fas fa-arrow-down" style="color:red;"></i></td>
                </tr>
                <tr>
                    <td>24.01.2020 00:16:11</td>
                    <td>78%</td>
                    <td><i class="fas fa-arrow-up" style="color:green;"></i></td>
                </tr>
                <tr class="bg-gray-200">
                    <td>23.01.2020 00:17:11</td>
                    <td>71%</td>
                    <td><i class="fas fa-arrow-up" style="color:green;"></i></td>
                </tr>
                </tbody>
                <tfoot>
                <tr>
                    <td colspan="3" class="bg-gray-500 text-center">
                        <a class="text-blue-200 font-bold">See All</a>
                    </td>
                </tr>
                </tfoot>
            </table>
          </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow">
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
    </div>
    <div v-if="loaded" class="segment shadow -mt-3">
        <div v-if="viewMode === 0" class="w-full">
            <div v-for="(scan, i) in scans" :key="`scan${i}`" class="w-full scan-list-item">
                {{ scan.target }}
            </div>
        </div>
        <div v-if="viewMode === 1" class="w-full flex flex-wrap">
            <div v-for="(scan, i) in scans" :key="`scan${i}`" class='scan-detailed-item'>
                  {{ scan.target }}
            </div>
        </div>
    </div>
  </div>
</template>

<script lang="ts" src="./Overview.ts"></script>
<style lang="scss" src="./Overview.scss"></style>