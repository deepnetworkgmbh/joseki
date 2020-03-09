<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary1.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.summary1.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.summary1.current.score) }}</div>
          </div>
        </div>
      </div>
      <div class="w-1/4">
        <div id="overall_pie1" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 top-seperator">
        <div id="overall_pie2" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3"><i :class="getScoreIconClass(data.summary2.current.score)"></i></div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">{{ date2 | formatDate }}</div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Score:</div>
            <div class="w-5/12 font-hairline text-left">{{ data.summary2.current.score }}%</div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">Grade:</div>
            <div class="w-5/12 font-hairline text-left">{{ getGrade(data.summary2.current.score) }}</div>
          </div>
        </div>
      </div>  
    </div>
    
    <div v-if="loaded" class="segment shadow" style="flex-direction:row">
        <div class="w-1/2" style="min-height:200px;">
            <div style="float:right;padding:10px">
              <a class='btn' :href="scanDetail1url">
                 <span class="px-2"><i class="fas fa-external-link-alt pr-2"></i>Scan Detail</span>
              </a>
            </div>
              <ul v-for="(collection,i) in data.results" :key="`collection1${i}`">
                <li>
                  <input type="checkbox" :id="`target1${i}`" checked />
                  <label :for="`target1${i}`" class="target">
                    <strong>{{ collection.type }}</strong> : {{ collection.name }}
                      <!-- <span class="text-xs scan-score">
                        score <strong>{{collection.score1}}%</strong>
                      </span> -->
                  </label>
                  <ul v-for="(obj, g) in collection.objects" :key="`obj1${i}-${g}`">
                    <li>
                      <input type="checkbox" :id="`obj1-${i}-${g}`" checked />
                      <label :for="`obj1-${i}-${g}`" class="text-base">
                        <strong>{{ obj.type }} : </strong> {{ obj.name }}
                      </label>
                      <!-- <span class="text-xs scan-score">
                        score <strong>{{ obj.score1}}%</strong>
                      </span> -->
                      <ul v-for="(control, c) in obj.controls" 
                        :key="`control${i}-${g}-${c}`">
                        <li>
                          <label :for="`control${i}-${g}-${c}`" class="text-sm">
                            <i :class="control.icon1"></i>
                            {{ control.result1 }}
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
        <div class="w-1/2 bottom-seperator" style="min-height:200px;">
            <div style="float:right;padding:10px">
              <a class='btn' :href="scanDetail2url">
                <span class="px-2"><i class="fas fa-external-link-alt pr-2"></i>Scan Detail</span>
              </a>
            </div>
             <ul v-for="(collection,i) in data.results" :key="`collection2${i}`">
                <li>
                  <input type="checkbox" :id="`target2${i}`" checked />
                  <label :for="`target2${i}`" class="target">
                    <strong>{{ collection.type }}</strong> : {{ collection.name }}
                      <!-- <span class="text-xs scan-score">
                        score <strong>{{collection.score2}}%</strong>
                      </span> -->
                  </label>
                  <ul v-for="(obj, g) in collection.objects" :key="`obj2${i}-${g}`">
                    <li>
                      <input type="checkbox" :id="`obj2-${i}-${g}`" checked />
                      <label :for="`obj2-${i}-${g}`" class="text-base">
                        <strong>{{ obj.type }} : </strong> {{ obj.name }}
                      </label>
                      <!-- <span class="text-xs scan-score">
                        score <strong>{{ obj.score2}}%</strong>
                      </span> -->
                      <ul v-for="(control, c) in obj.controls" 
                        :key="`control${i}-${g}-${c}`">
                        <li>
                          <label :for="`control${i}-${g}-${c}`" class="text-sm">
                            <i :class="control.icon2"></i>
                            {{ control.result2 }}
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
      <!--<h1 class="mb-2">Results by Resources</h1>
      <hr class='mb-2' />

       <ul v-for="(collection,i) in Object.keys(ResultsByCollection)" :key="collection">
          <li>
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`" class="target">
               <strong>{{ ResultsByCollection[collection].type }}</strong> : {{ ResultsByCollection[collection].name }}
                <span class="text-xs">
                  score <strong>{{ResultsByCollection[collection].score}}%</strong>
                </span>
            </label>
            <StatusBar :counters="ResultsByCollection[collection].counters" />
            <ul v-for="(obj, g) in Object.keys(ResultsByCollection[collection].objects)" :key="`obj${i}-${g}`">
              <li>
                <input type="checkbox" :id="`obj${i}-${g}`" />
                <label :for="`obj${i}-${g}`" class="text-base">
                   <strong>{{ ResultsByCollection[collection].objects[obj].type }} : </strong>
                   {{ ResultsByCollection[collection].objects[obj].name }}
                </label>
                <StatusBar :mini='false' :counters="ResultsByCollection[collection].objects[obj].counters" style="margin-right:-10px;" />
                <ul v-for="(control, c) in ResultsByCollection[collection].objects[obj].controls" 
                   :key="`control${i}-${g}-${c}`">
                  <li>
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
        </ul>  -->
    </div>
   
  </div>
</template>

<script lang="ts" src="./ComponentDiff.ts"></script>
<style lang="scss" src="./ComponentDiff.scss"></style>