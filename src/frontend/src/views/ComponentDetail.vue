<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div
        class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center"
        style="overflow:hidden;"
      >
        <div class="status-icon">
          <i :class="getScoreIconClass(data.current.score)"></i>
        </div>
        <div class="status-text">
          <div
            class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4"
          >{{ selectedDate | formatDate }}</div>

          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Type:</div>
            <div class="w-6/12 font-hairline text-left">{{ data.component.category }}</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-sm">
            <div class="w-4/12 font-thin text-right mr-1 text-gray-600">Id:</div>
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
          <div class='text-center text-xs font-bold'>Scan History</div>
          <div id="overall_bar" style="width:100%"></div>
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
            <tfoot class='border-t border-gray-500'>
              <tr>
                <td colspan="3" class="text-right">
                  <button class="btn mt-2" @click="goComponentHistory()">
                    <span class="px-4"><i class="fas fa-history pr-2"></i>See Scan History</span>
                  </button>
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow" style="flex-direction:column">
      <h1 class="mb-2">Results By Category</h1>
      <hr class='mb-2' />
      <div v-for="(category,i) in ResultsByCategory" :key="`category${i}`">
        <ul>
          <li>
            <input type="checkbox" v-bind:id="`cat${i}`" />
            <label class="text-base" v-bind:for="`cat${i}`">
              <strong>{{category.category}}</strong>
              <Score :label='`Score`' :score='category.score' />              
            </label>
            <StatusBar :counters="category.counters" />
            <ul>
             ???
            </ul>
          </li>
        </ul>
      </div>      
    </div>
    <div v-if="loaded" class="segment shadow" style="flex-direction:column">
      <h1 class="mb-2">Results by Resources</h1>
      <hr class='mb-2' />
      <ul v-for="(collection,i) in ResultsByCollection" :key="`collection${i}`">
          <li>
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`" class="target">
               <strong>{{ collection.type }}</strong> : {{ collection.name }}
               <Score :label='`Score`' :score='collection.score' />             
            </label>
            <StatusBar :counters="collection.counters" />
            <ul v-for="(obj, g) in collection.objects" :key="`obj${i}-${g}`" style="border-bottom: dashed 1px #eee;">
              <li style="margin-top:5px;margin-left:5px;">
                <input type="checkbox" :id="`obj${i}-${g}`" />
                <label :for="`obj${i}-${g}`" class="text-base">
                   <strong>{{ obj.type }} : </strong>
                   {{ obj.name }}
                </label>
                <Score :label='`Score`' :score='obj.score' />         
                <StatusBar :mini='false' :counters="obj.counters" style="margin-right:-5px;" />
                <ul v-for="(control, c) in obj.controls" 
                   :key="`control${i}-${g}-${c}`" style="border-bottom: dashed 1px #eee;">
                  <li style="margin-left:5px;">
                    <label :for="`control${i}-${g}-${c}`" class="text-sm">
                       <i :class="control.icon"></i>
                      <i>{{ control.id }}</i> : {{ control.text }}
                       <span class="tool" :data-tip="control.text">
                        <i class="far fa-question-circle tip-icon"></i>
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