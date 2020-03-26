<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-show="loaded" class="segment shadow" style="min-height:300px;padding:0">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel"
        style="overflow:hidden;">
        <div class="status-icon">
          <i :class="getScoreIconClass(data.current.score)"></i>
        </div>
        <div class="status-text p-5 pl-4 pt-16">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ selectedDate | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.current.score) }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Type</h5>
            <h1 class="info">{{ data.component.category }}</h1>
          </div>
          <div class="mb-3 info-tag">
            <h5>Name</h5>
            <h1 class="info">{{ data.component.name }}</h1>
          </div>
        </div>
      </div>
      <div class="w-2/4 pt-8">
        <div id="overall_pie" class="w-auto" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 top-right-panel" style="z-index:10;">
        <div class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-sm font-bold'>Scan History</div>
          <div id="overall_bar" style="width:100%"></div>
        </div>
        <div class="m-3 mt-0">
          <div class='text-center text-sm font-bold border-b border-gray-500'>Last 5 scans</div>
          <table class="w-full text-xs p-4">
            <tbody>
              <tr v-for="(scan,i) in shortHistory" :key="`scan${i}`" 
              @click="scan.score > 0 ? dayClicked(scan.recordedAt.split('T')[0], data.component.id) : undefined" 
              :class='getHistoryClass(scan)'>
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
    <div v-show="loaded" class="segment shadow" style="flex-direction:column">
       <div class="segment-header">
        <h1 class="mb-2">Results by Category</h1>
      </div>
      <div v-for="(category,i) in getResultsByCategory(data)" :key="`category${i}`" class="zigzag">
        <ul>
          <li style="margin-left:10px;min-height:30px;padding-top:3px;">
            <input type="checkbox" v-bind:id="`cat${i}`" />
            <label class="text-base" v-bind:for="`cat${i}`">
              <strong>{{category.category}}</strong>
              <Score :label='`Score`' :score='category.score' />              
            </label>
            <div style="float:right;"><StatusBar :counters="category.counters" /></div>
            <ul>
               <div style="padding:4px;padding-left:10px;padding-right:220px;text-align:justify" 
                   v-html="getCategoryMeta(category.category)" v-linkified:options="{ className: 'external-link' }" />              
            </ul>
          </li>
        </ul>
      </div>      
    </div>
    <div v-show="loaded" class="segment shadow" style="flex-direction:column">
      <div class="segment-header">
        <h1 class="mb-2">Results by Resources</h1>
      </div>

      <ul v-for="(collection,i) in getResultsByCollection(data)" :key="`collection${i}`"  class="zigzag">
          <li style="margin-left:10px;min-height:30px;">
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`">
               <strong>{{ collection.type }} :</strong>
               <Score :label='`Score`' :score='collection.score' />             
                {{ collection.name }}
            </label>
            <div style="float:right;margin-top:3px;"><StatusBar :counters="collection.counters" /></div>
            <ul v-for="(obj, g) in collection.objects" :key="`obj${i}-${g}`">
              <li style="margin-left:15px;">
                <input type="checkbox" :id="`obj${i}-${g}`" />
                <label :for="`obj${i}-${g}`" class="limited-label">
                   <strong>{{ obj.type }} :</strong>
                   <Score :label='`Score`' :score='obj.score' />         
                   {{ obj.name }}
                </label>
                <!-- <StatusBar :mini='false' :counters="obj.counters" style="margin-right:-5px;margin-top:-27px;" />  -->
                <!-- list with subgroup -->
                <ul v-for="(control, c) in obj.controlGroups" 
                  :key="`control${i}-${g}-${c}`" class="scan-control">
                  <li style="padding:2px;padding-left:0;margin-left:5px;margin-top:0px;margin-bottom:2px;">

                    <b>{{control.name}} ({{ control.items.length }})</b>
                    <div v-for="(cg, cgi) in control.items" :key='`cgi${i}-${g}-${c}-${cgi}`'>
                      <label :for="`control${i}-${g}-${c}`" class="text-sm">
                        <i :class="cg.icon"></i> {{ cg.result }} : {{ cg.id }}                        
                        <span class="ml-1 mr-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="cg.text">
                          <span class="icon-help-circle tip-icon"></span>
                        </span>
                        <span v-if="cg.id === 'container_image.CVE_scan' && cg.text !== 'No issues'">                        
                          <router-link class='small-link' :to="{ name: 'ImageDetail', params: { imageid: cg.tags.imageTag, date: date, component: data.component }}">see details</router-link>
                        </span>
                      </label>
                    </div>
                  </li>
                </ul>
                <!-- list with no subgroup -->
                <ul v-for="(control, c) in obj.controls" 
                  :key="`control${i}-${g}-${c}`" class="scan-control">
                  <li style="padding:2px;padding-left:0;margin-left:5px;margin-top:0px;margin-bottom:2px;">
                      <label :for="`control${i}-${g}-${c}`" class="text-sm">
                        <i :class="control.icon"></i> {{ control.result }} : {{ control.id }}
                        <span class="ml-1 mr-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="control.text">
                          <span class="icon-help-circle tip-icon"></span>
                        </span>
                        <span v-if="control.id === 'container_image.CVE_scan' && control.text !== 'No issues'">                        
                          <a class='small-link' :href="imageScanUrl(control.tags.imageTag)">see details</a>
                        </span>
                      </label>
                  </li>
                </ul>
              </li>
            </ul>
          </li>
        </ul> 
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentDetail.ts"></script>
<style lang="scss" src="./ComponentDetail.scss"></style>