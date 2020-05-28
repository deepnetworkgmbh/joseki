<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel"
        style="overflow:hidden;">
        <div class="status-text p-5 pl-4 pt-4">
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
      <div class="w-2/4">
        <apexchart :options="getPieChartOptions()" :series="getPieChartSeries()"></apexchart>
      </div>
      <div class="w-1/4 border-l border-gray-300 top-right-panel" style="z-index:10;">
        <div class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-sm font-bold'>Scan History</div>
          <div style="width:100%;height:70px;">
            <apexchart height="70" :options="getAreaChartOptions()" :series="getAreaSeries()"></apexchart>
          </div>
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
            <button class="btn mt-2 gradient" @click="goComponentHistory()">
              <span class="px-4"><span class="icon-more-vertical pr-1"></span>See Scan History</span>
            </button>
          </div>  
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow" style="flex-direction:column">
      <div class="segment-header">
        <h1 class="mb-2">Results by Category</h1>
      </div>
      <div v-for="(category,i) in getResultsByCategory()" :key="`category${i}`" class="zigzag">
        <ul>
          <li style="margin-left:10px;min-height:30px;padding-top:3px;">
            <StatusBar :counters="category.counters" style="float:right;margin-top:-17px;" />
            <input class="expand" type="checkbox" v-bind:id="`cat${i}`" />
            <label class="text-base" v-bind:for="`cat${i}`">
              <strong>{{category.category}}</strong>
              <Score :label='`Score`' :score='category.score' />              
            </label>
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
        <h1 class="mb-2">Results by Collection</h1>
      </div>
      <div class="component-detail">
        <div class="component-detail-search" style="height:30px;padding:2px;padding-top:5px;background-color:#eee;">
          <AdvancedFilter style="float:left" :channel="'component'" :filter="filter" @filterUpdated="onFilterChangedFromAF" />
        </div>
        <div v-for="(collection,i) in resultsByCollection" :key="`collection${i}`" class="component-detail-collection">
            <div class="component-detail-collection-row" @click="toggleCollectionChecked(i)">
              <div class="component-detail-collection-row-arrow">
                <i :class="collection.checked ? 'icon-chevron-down' : 'icon-chevron-right'"></i>
              </div>
              <div class="component-detail-collection-row-name">  
                <strong>{{ collection.type }} :</strong> {{ collection.name }}  
              </div>
              <div style="padding-top:1px;">
                <span v-if="collection.owner" class="owner-tag"><i class="icon-user"></i> {{ collection.owner }}</span>
                <!-- <StatusBar :counters="collection.counters" :severities='severityFilter' /> -->
              </div>
              <div class="component-detail-collection-row-score">
                <Score v-if="getShowScore()" :label='`Score`' :score='collection.score' />
              </div>
            </div>
            <div v-if="collection.checked">
              <div class="component-detail-collection-row-objects" v-for="(obj, g) in collection.objects" :key="`obj${i}-${g}`">
                <div class="component-detail-collection-row-objects-row"  @click="toggleObjectChecked(i, g)">
                  <div class="component-detail-collection-row-objects-row-arrow">
                    <i :class="obj.checked ? 'icon-chevron-down' : 'icon-chevron-right'"></i>
                  </div>
                  <div class="component-detail-collection-row-objects-row-name">
                      <strong>{{ obj.type }} : </strong>{{ obj.name }}
                  </div>
                  <div style="padding-top:1px;">
                    <span v-if="collection.owner" class="owner-tag"><i class="icon-user"></i> {{ collection.owner }}</span>                
                  </div>
                  <div class="component-detail-collection-row-objects-row-score">
                    <Score v-if="getShowScore()" :label='`Score`' :score='obj.score' />
                  </div>
                </div>
                <div v-if="obj.checked">
                  <div v-for="(control, c) in obj.controls" class="component-detail-collection-row-objects-row-control zigzag" :key="`control${i}-${g}-${c}`">
                    <div :class="`component-detail-collection-row-objects-row-control-icon resultBG${control.result}`">
                      <i :class="control.icon" style="font-size:13px"></i> 
                    </div>
                    <div :class="`component-detail-collection-row-objects-row-control-result resultBG${control.result}`">
                      {{ control.result }}
                    </div>
                      <div class="component-detail-collection-row-objects-row-control-category">
                        {{ control.category }}
                      </div>
                    <div class="component-detail-collection-row-objects-row-control-details">
                      <span v-if="control.id === 'container_image.CVE_scan' && control.text !== 'No issues'">                        
                          <a class='small-link' :href="imageScanUrl(control.tags.imageTag)">see details</a>
                        </span>
                        <span v-else>
                          <router-link class='small-link' :to="{ name: 'CheckDetail', params: { checkid: control.id, date: date, component: data.component }}">{{ control.id }}</router-link>  
                        </span>
                        <span class="ml-1 mr-1 pt-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="control.text">
                          <i class="icon-help-circle tip-icon"></i>
                        </span>
                    </div>
                  </div>
                  <div v-for="(control, c) in obj.controlGroups" class="component-detail-collection-row-objects-row-controlgroup zigzag" :key="`control${i}-${g}-${c}`">
                    <div class="component-detail-collection-row-objects-row-controlgroup-name">
                      <b>{{control.name}} ({{ control.items.length }})</b>
                    </div>
                    <div v-for="(cg, cgi) in control.items" :key='`cgi${i}-${g}-${c}-${cgi}`' class="component-detail-collection-row-objects-row-controlgroup-items">
                      <div :class="`component-detail-collection-row-objects-row-controlgroup-icon resultBG${cg.result}`">
                        <i :class="cg.icon" style="font-size:13px"></i> 
                      </div>
                      <div :class="`component-detail-collection-row-objects-row-controlgroup-result resultBG${cg.result}`">
                        {{ cg.result }}
                      </div>
                      <div class="component-detail-collection-row-objects-row-controlgroup-category">
                        {{ cg.category }}
                      </div>
                      <div class="component-detail-collection-row-objects-row-controlgroup-details">
                        <span v-if="cg.id === 'container_image.CVE_scan' && cg.text !== 'No issues'">                        
                          <router-link class='small-link' :to="{ name: 'ImageDetail', params: { imageid: cg.tags.imageTag, date: date, component: data.component }}">
                            see image scan details for <b>{{cg.tags.imageTag}}</b>
                          </router-link>
                        </span>
                        <span v-else>
                          <router-link class='small-link' :to="{ name: 'CheckDetail', params: { checkid: cg.id, date: date, component: data.component }}">{{ cg.id }}</router-link>  
                        </span>
                        <span class="ml-1 mr-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="cg.text">
                          <span class="icon-help-circle tip-icon"></span>
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentDetail.ts"></script>
<style lang="scss" src="./ComponentDetail.scss"></style>