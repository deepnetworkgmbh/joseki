<template>
  <div>
    <Spinner v-if="!loaded" class="centered" />
    <div v-show="loaded" class="segment shadow" style="display: flex;flex-direction: row;">
      <div style="padding: 5px;" id="chart" ref="chart1"></div>
      <div style="padding: 5px;display: flex;flex-direction: column;justify-content: center;">
        <div>
          This cluster uses
          <b>{{ data.length }} unique container images</b>:
        </div>
        <ul>
          <li v-for="group in data.groups" v-bind:key="group.title">
            <i class="fas fa-arrow-right" style="color:#aaa;margin-right:3px"></i>
            <b class="'severity-' + group.title">{{ group.count }}</b>
            {{group.description}}
          </li>
        </ul>
      </div>
    </div>
    <div v-show="loaded" class="segment shadow">
      <div class="segment-toolbar">
        <div style="float:right;">
          Group By
          <select @change="renderList()" v-model="selectedAttribute">
            <option
              v-for="grp in groupByOptions"
              v-bind:key="grp.value"
              v-bind:value="grp.value"
            >{{grp.text}}</option>
          </select>
        </div>
        <div style="float:left;">
          <div>
            <input type="search" class="searchbox" onkeyup="search(this)" />
            <i class="fas fa-search search-icon"></i>
          </div>
        </div>
      </div>
      <div style="margin-top:15px;">
        <div v-for="(result,i) in results" v-bind:key="result.title">
          <ul>
            <li>
              <input type="checkbox" v-bind:id="`target${i}`" checked />
              <label v-bind:for="`target${i}`">{{ result.title }} ({{ result.images.length }})</label>
              <StatusBar :counters="result.counter" />
              <ul>
                <li v-for="(image,j) in result.images" :key="`sub${j}`" class="sub-item">
                  <div style="float:right;" v-html="image.rowText"></div>
                  <i :class="image.icon"></i>
                  {{ image.shortImageName }}
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
</template>
<script lang="ts" src="./ImageOverview.ts"></script>
<style lang="scss" src="./ImageOverview.scss"></style>