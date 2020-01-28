<template>
  <div>
    <Spinner v-if="!loaded" />
    <div v-show="loaded" class="segment" style="min-height: 600px">
      <div class="w-1/3 justify-center rounded-lg mr-2">
        <div id="chart" ref="chart1" class="p-2"></div>
        <div class="pl-10 mt-3">
          <div>
            This cluster uses
            <b>{{ data.length }} unique container images</b>:
          </div>
          <ul class="mt-2">
            <li v-for="group in data.groups" v-bind:key="group.title">
              <i class="fas fa-arrow-right" style="color:#aaa;margin-right:3px"></i>
              <b :class="`severity-${group.title}`">{{ group.count }}</b>
              {{group.description}}
            </li>
          </ul>
        </div>
      </div>
      <div class="w-2/3 flex flex-col">
        <div class="flex flex-row p-3 rounded-t-lg bg-blue-200 border-b border-gray-400">
          <div class="w-1/2 mr-2">
            <input
              type="search"
              class="w-full px-2 rounded-lg"
              style="float:left;"
              v-model="filter"
              v-on:keyup="renderList()"
            />
            <i
              class="fas fa-search text-gray-200"
              style="float:right;margin-top:-20px;margin-right:9px;"
            ></i>
          </div>
          <div class="w-1/2 flex flex-row">
            <div class="w-1/2 text-right mr-2">Group By</div>
            <div class="w-1/2">
              <select @change="renderList()" v-model="selectedAttribute" class="text-sm w-48">
                <option
                  v-for="grp in groupByOptions"
                  class="text-xl"
                  v-bind:key="grp.value"
                  v-bind:value="grp.value"
                >{{grp.text}}</option>
              </select>
            </div>
          </div>
        </div>
        <div
          class="bg-gray-100 pt-4 rounded-b-lg border border-gray-200"
          style="min-height: 580px;z-index:0"
        >
          <div v-for="(result,i) in results" v-bind:key="result.title">
            <ul>
              <li v-if="result.images.length>0">
                <input type="checkbox" v-bind:id="`target${i}`" checked />
                <label
                  class="text-base"
                  v-bind:for="`target${i}`"
                  v-html="`${result.title} (${result.images.length})`"
                ></label>
                <StatusBar :counters="result.counter" />
                <ul>
                  <li v-for="(image,j) in result.images" :key="`sub${j}`">
                    <div style="float:right;" v-html="image.rowText" class="text-xs"></div>
                    <i :class="image.icon"></i>
                    <span v-html="image.shortImageName" class="text-sm"></span>
                    <router-link :to="image.link" class="more-info">
                      <span class="tool" :data-tip="image.tip">
                        <i class="far fa-question-circle tip-icon"></i>
                      </span>
                    </router-link>
                  </li>
                </ul>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./ImageOverview.ts"></script>
<style lang="scss" src="./ImageOverview.scss"></style>