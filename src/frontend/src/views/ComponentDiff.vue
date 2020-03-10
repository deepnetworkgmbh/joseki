<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px">
      <div
        class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center"
        style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3">
          <i :class="getScoreIconClass(data.summary1.current.score)"></i>
        </div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">
            {{ date | formatDate }}
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">
              Score:
            </div>
            <div class="w-5/12 font-hairline text-left">
              {{ data.summary1.current.score }}%
            </div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">
              Grade:
            </div>
            <div class="w-5/12 font-hairline text-left">
              {{ getGrade(data.summary1.current.score) }}
            </div>
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
        <div class="status-icon ml-1 mt-3">
          <i :class="getScoreIconClass(data.summary2.current.score)"></i>
        </div>
        <div class="status-text">
          <div class="p-1 m-auto rounded-sm text-lg text-center -mt-8 mb-2 pb-4">
            {{ date2 | formatDate }}
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">
              Score:
            </div>
            <div class="w-5/12 font-hairline text-left">
              {{ data.summary2.current.score }}%
            </div>
          </div>
          <div class="flex flex-row xl:text-xl lg:text-lg md:text-md sm:text-sm">
            <div class="w-7/12 font-thin text-right mr-1 text-gray-600">
              Grade:
            </div>
            <div class="w-5/12 font-hairline text-left">
              {{ getGrade(data.summary2.current.score) }}
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- <a class='btn' :href="scanDetail1url">
                 <span class="px-2"><i class="fas fa-external-link-alt pr-2"></i>Scan Detail</span>
    </a>-->
    <div v-if="loaded" class="segment shadow" style="flex-direction:row">
      <div class="w-full" style="min-height:200px;">
        <ul v-for="(row, i) in data.results" :key="`collection1${i}`">
          <li :class='getWrapperClass(row.operation)'>
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`" class='diff-row-label'>
              <strong>{{ row.type }} : {{ row.name }}</strong> 
              <span class='diff-row-change-text'>{{ getRowTitle(row.operation, row.changes) }}</span>
              <div :class="getRowClass(row.operation)">
                <div class="diff-cell">
                  <ul v-for="(obj, g) in row.left.objects" :key="`left${i}-${g}`">
                    <li style="margin-left:15px;">
                      <input type="checkbox"  :id="`left-obj-${obj.id}`" v-model="obj.checked" 
                           @click="toggleOther(`left-obj-${obj.id}`, row.key, obj.id)" />
                      <label :for='`left-obj-${obj.id}`' :class='getObjectClass(obj.operation)'>
                        <strong>{{ obj.type }} :</strong> {{ obj.name }} {{ obj.operation}}
                        <Score :label="`Score`" :score="obj.score" />
                        <div v-if="obj.checked">
                          <ul v-for="(control, c) in obj.controls" :key="`left-controlp${i}-${g}-${c}`" class="control-ul">
                            <ControlList :date="row.left.date" :control="control" />
                          </ul>
                          <ul v-for="(cg, c) in obj.controlGroups" :key="`left-controlg${i}-${g}-${c}`" class="control-ul">
                            <ControlGroup :date="row.left.date" :name="cg.name" :items="cg.items" />
                          </ul>
                        </div>
                      </label>
                    </li>
                  </ul>
                </div>
                <div class="diff-cell ml-1">
                  <ul v-for="(obj, g) in row.right.objects" :key="`right${i}-${g}`">
                    <li style="margin-left:15px;">
                      <input type="checkbox" :id="`right-obj-${obj.id}`" v-model="obj.checked" 
                           @click="toggleOther(`right-obj-${obj.id}`, row.key, obj.id)" />
                      <label :for='`right-obj-${obj.id}`' :class='getObjectClass(obj.operation)'>
                        <strong>{{ obj.type }} :</strong>  {{ obj.name }}  {{ obj.operation}} 
                        <Score :label="`Score`" :score="obj.score" />
                        <div v-if="obj.checked">
                          <ul v-for="(control, c) in obj.controls" :key="`right-controlp${i}-${g}-${c}`" class="control-ul">
                            <ControlList :date="row.left.date" :control="control" />
                          </ul>
                          <ul v-for="(cg, c) in obj.controlGroups" :key="`right-controlg${i}-${g}-${c}`" class="control-ul">
                            <ControlGroup :date="row.left.date" :name="cg.name" :items="cg.items" />
                          </ul>
                        </div>
                      </label>
                    </li>
                  </ul>

                </div>                
              </div>
            </label>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentDiff.ts"></script>
<style lang="scss" src="./ComponentDiff.scss"></style>


