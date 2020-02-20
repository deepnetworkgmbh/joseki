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
        <div id="overall_bar" class="w-auto border-b border-gray-500 p-2 ml-1 mb-3"></div>
        <div class="text-center">
          <button class="btn" @click="goComponentHistory()">See All Scan History</button>
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow" style="flex-direction:column">
      <h1 class="mb-2">Results By Category</h1>
      <hr class='mb-2' />
      <div v-for="(category,i) in Object.keys(ResultsByCategory)" :key="category">
        <ul>
          <li>
            <input type="checkbox" v-bind:id="`cat${i}`" />
            <label class="text-base" v-bind:for="`cat${i}`">
              <strong>{{category}}</strong>              
              <span class="text-xs">
              score <strong>{{ResultsByCategory[category].score}}%</strong>
              </span>
            </label>
            <StatusBar :counters="ResultsByCategory[category]" />
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
                <input type="checkbox" :id="`obj${i}-${g}`" checked />
                <label :for="`obj${i}-${g}`" class="text-base">
                   <strong>{{ ResultsByCollection[collection].objects[obj].type }} : </strong>
                   {{ ResultsByCollection[collection].objects[obj].name }}
                </label>
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
        </ul> 

  </div>
  </div>
</template>

<script lang="ts" src="./ComponentDetail.ts"></script>
<style lang="scss" src="./ComponentDetail.scss"></style>