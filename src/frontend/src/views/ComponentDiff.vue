<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-if="loaded" class="segment shadow" style="min-height:300px;padding:0">
      <div class="w-1/4 border-r border-gray-300 flex flex-col justify-center content-center top-left-panel" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3">
          <i :class="getScoreIconClass(data.summary1.current.score)"></i>
        </div>
        <div class="status-text p-5 pl-4 pt-16">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ date | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.summary1.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.summary1.current.score) }}</h1>
          </div>
          <div class="text-center mt-2 pt-2">
            <a class='btn' :href="scanDetail1url"><span class="px-2"><i class="icon-arrow-up-right pr-2"></i>Scan Detail</span></a>
          </div>       
        </div>
      </div>
      <div class="w-1/4 pt-4">
        <div id="overall_pie1" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 pt-4 top-seperator">
        <div id="overall_pie2" class="w-auto mt-4" style="z-index:0;"></div>
      </div>
      <div class="w-1/4 border-l border-gray-300 flex flex-col justify-center content-center top-right-panel" style="overflow:hidden;">
        <div class="status-icon ml-1 mt-3">
          <i :class="getScoreIconClass(data.summary2.current.score)"></i>
        </div>
        <div class="status-text p-5 pl-4 pt-16">
          <div class="mb-3 info-tag-date">
            <h5>Date</h5>
            <h1 class="info">{{ date2 | formatDate }}</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Score</h5>
            <h1 class="info">{{ data.summary2.current.score }}%</h1>
          </div>
          <div class="mb-3 info-tag-score">
            <h5>Grade</h5>
            <h1 class="info">{{ getGrade(data.summary2.current.score) }}</h1>
          </div>
          <div class="text-center mt-2 pt-2">
            <a class='btn' :href="scanDetail2url"><span class="px-2"><i class="icon-arrow-up-right pr-2"></i>Scan Detail</span></a>
          </div>       
        </div>
      </div>
    </div>
    <div v-if="loaded" class="segment shadow" style="flex-direction:row">
      <div v-if="!nochanges" class="w-full" style="min-height:80px;">
        <ul v-for="(row, i) in data.results" :key="`collection1${i}`">
          <li v-if="row.operation !== 'SAME'" :class='getWrapperClass(row)'>
            <input type="checkbox" :id="`target${i}`" v-model="row.checked"  />
            <label :for="`target${i}`" class='diff-row-label'>
              <strong>{{ row.type }} : {{ row.name }} </strong> 
              <span class='diff-row-change-text'>{{ row.operation }}</span>
            </label>
            <div v-if="row.checked" :class="getRowClass(row.operation)">
              <div class="diff-cell">
                <ul v-for="(obj, g) in row.left.objects" :key="`left${i}-${g}`" :class="getWrapperClass(obj)">
                  <li v-if="obj.operation !== 'SAME'" :class='getObjectContainerClass(obj)'>
                    <input type="checkbox" :id="`left-obj-${obj.id}`" v-model="obj.checked" 
                          @click="toggleOther(`left-obj-${obj.id}`, row.key, obj.id)" />
                    <label :for='`left-obj-${obj.id}`'>
                      <span v-if='!obj.empty'><strong>{{ obj.type }} :</strong> {{ obj.name }} </span>                     
                      <span class="diff-tag">{{ obj.operation }}</span>
                    </label>
                    <div v-if="obj.checked">
                      <ul v-for="(control, c) in obj.controls" :key="`left-controlp${i}-${g}-${c}`" class="control-ul">
                        <ControlList :date="row.left.date" :control="control" />
                      </ul>
                      <ul v-for="(cg, c) in obj.controlGroups" :key="`left-controlg${i}-${g}-${c}`" class="control-ul">
                        <ControlGroup  v-if="cg.operation !== 'SAME'" :date="row.left.date" :name="cg.name" :items="cg.items" :operation="cg.operation" />
                      </ul>
                    </div>
                  </li>
                </ul>
              </div>
              <div class="diff-cell">
                <ul v-for="(obj, g) in row.right.objects" :key="`right${i}-${g}`" :class="getWrapperClass(obj)">
                  <li v-if="obj.operation !== 'SAME'" :class='getObjectContainerClass(obj)'>
                    <input type="checkbox" :id="`right-obj-${obj.id}`" v-model="obj.checked" 
                          @click="toggleOther(`right-obj-${obj.id}`, row.key, obj.id)" />
                    <label :for='`right-obj-${obj.id}`'>                        
                      <span v-if='!obj.empty'><strong>{{ obj.type }} :</strong> {{ obj.name }}</span>       
                      <span class="diff-tag">{{ obj.operation }}</span>             
                    </label>
                    <div v-if="obj.checked">
                      <ul v-for="(control, c) in obj.controls" :key="`right-controlp${i}-${g}-${c}`" class="control-ul">
                        <ControlList :date="row.right.date" :control="control" />
                      </ul>
                      <ul v-for="(cg, c) in obj.controlGroups" :key="`right-controlg${i}-${g}-${c}`" class="control-ul">
                        <ControlGroup v-if="cg.operation !== 'SAME'" :date="row.right.date" :name="cg.name" :items="cg.items" :operation="cg.operation" />
                      </ul>
                    </div>
                  </li>
                </ul>

              </div>                
            </div>
          </li>
        </ul>
      </div>
      <div v-else>
        <div class="text-center">No differences found between two scans.</div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./ComponentDiff.ts"></script>
<style lang="scss" src="./ComponentDiff.scss"></style>


