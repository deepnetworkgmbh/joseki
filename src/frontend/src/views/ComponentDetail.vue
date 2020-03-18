<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-show="loaded" class="segment shadow" style="min-height:300px">
      <div
        class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center"
        style="overflow:hidden;"
      >
        <div class="status-icon">
          <i :class="getScoreIconClass(data.current.score)"></i>
        </div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ selectedDate | formatDate }}</div>

          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Type:</div>
            <div class="w-6/12 font-hairline text-left">{{ data.component.category }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Name:</div>
            <div class="w-6/12 font-hairline text-left">{{ data.component.name }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-6/12 font-hairline text-left">{{ data.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-6/12 font-hairline text-left">{{ getGrade(data.current.score) }}</div>
          </div>
        </div>
      </div>
      <div class="w-2/4">
        <div id="overall_pie" class="w-auto" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300" style="z-index:10;">
        <div class="w-auto p-2 ml-1 mb-2">
          <div class='text-center text-sm font-bold'>Scan History</div>
          <div id="overall_bar" style="width:100%"></div>
        </div>
        <div class="m-3 mt-0">
          <div class='text-center text-sm font-bold border-b border-gray-500'>Last 5 scans</div>
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
    <div v-show="loaded" class="segment shadow" style="flex-direction:column">
      <h1 class="mb-2">Results By Category</h1>
      <hr class='mb-2' />
      <div v-for="(category,i) in getResultsByCategory(data)" :key="`category${i}`">
        <ul>
          <li>
            <input type="checkbox" v-bind:id="`cat${i}`" />
            <label class="text-base" v-bind:for="`cat${i}`">
              <strong>{{category.category}}</strong>
              <Score :label='`Score`' :score='category.score' />              
            </label>
            <StatusBar :counters="category.counters" />
            <ul>
               <div style="padding:4px;padding-left:10px;" v-html="getCategoryMeta(category.category)" v-linkified:options="{ className: 'external-link' }" />              
            </ul>
          </li>
        </ul>
      </div>      
    </div>
    <div v-show="loaded" class="segment shadow" style="flex-direction:column">
      <h1 class="mb-2">Results by Resources</h1>
      <hr class='mb-2' />

      <ul v-for="(collection,i) in getResultsByCollection(data)" :key="`collection${i}`">
          <li style="margin-left:10px;">
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`">
               <strong>{{ collection.type }} :</strong>
               <Score :label='`Score`' :score='collection.score' />             
                {{ collection.name }}
            </label>
            <StatusBar :counters="collection.counters" />
            <ul v-for="(obj, g) in collection.objects" :key="`obj${i}-${g}`">
              <li style="margin-left:15px;">
                <input type="checkbox" :id="`obj${i}-${g}`" />
                <label :for="`obj${i}-${g}`" class="limited-label">
                   <strong>{{ obj.type }} :</strong>
                   <Score :label='`Score`' :score='obj.score' />         
                   {{ obj.name }}
                </label>
                <StatusBar :mini='false' :counters="obj.counters" style="margin-right:-5px;margin-top:-27px;" />
                <!-- list with subgroup -->
                <ul v-for="(control, c) in obj.controlGroups" 
                  :key="`control${i}-${g}-${c}`" style="border:dashed 1px #eee;margin-left:10px;">
                  <li style="padding:2px;padding-left:0;margin-left:5px;margin-top:0px;margin-bottom:2px;">

                    <b>{{control.name}} ({{ control.items.length }})</b>
                    <div v-for="(cg, cgi) in control.items" :key='`cgi${i}-${g}-${c}-${cgi}`'>
                      <label :for="`control${i}-${g}-${c}`" class="text-sm">
                        <i :class="cg.icon"></i> {{ cg.result }} : {{ cg.id }}
                        <span class="ml-1 mr-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="cg.text">
                          <i class="far fa-question-circle tip-icon"></i>
                        </span>
                        <span v-if="cg.id === 'container_image.CVE_scan' && cg.text !== 'No issues'">                        
                        <a class='small-link' @click="goToImageScan(cg.tags.imageTag)">see details</a>                                               
                        </span>
                      </label>
                    </div>
                  </li>
                </ul>
                <!-- list with no subgroup -->
                <ul v-for="(control, c) in obj.controls" 
                  :key="`control${i}-${g}-${c}`" style="border:dashed 1px #eee;margin-left:10px;">
                  <li style="padding:2px;padding-left:0;margin-left:5px;margin-top:0px;margin-bottom:2px;">
                      <label :for="`control${i}-${g}-${c}`" class="text-sm">
                        <i :class="control.icon"></i>
                        {{ control.id }}
                        <span class="ml-1 mr-1" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="control.text">
                          <i class="far fa-question-circle tip-icon"></i>
                        </span>
                        <span v-if="control.id === 'container_image.CVE_scan' && control.text !== 'No issues'">                        
                        <a class='small-link' @click="goToImageScan(control.tags.imageTag)">see details</a>                                               
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