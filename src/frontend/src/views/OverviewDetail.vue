<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div
      v-if="loaded"
      class="shadow"
      style="min-height:300px;padding:4px;background-color:#fff;border-radius:4px;">
      <table class="table">
        <thead>
          <tr class="table-header">
            <td v-for="(col,i) in headers" :key="`col${i}`" :style="getColumnWidth(i)">
              <span style="cursor:pointer;" class="noselect" @click="changeOrdering(i)">
                {{ col.label }}
                <i v-if="col.sortable" :class="getHeaderClass(i)" />
              </span>
            </td>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item,i) in data.checks" :key="`item${i}`" class="checkrow">
            <td :style="getColumnWidth(0)" class="row-ellipsis">
              <i :class="getComponentIcon(item.component.category)" />
              {{ item.component.name }}
            </td>
            <td :style="getColumnWidth(1)" class="row-ellipsis">{{ item.category }}</td>
            <td :style="getColumnWidth(2)" class="row-ellipsis">{{ item.collection.type }}:{{ item.collection.name }}</td>
            <td :style="getColumnWidth(3)" class="row-ellipsis">{{ item.resource.type }}:{{ item.resource.name }}</td>
            <td :style="getColumnWidth(4)" class="row-ellipsis">
                <span v-if="item.resource.owner" class="owner-tag"><i class="icon-user"></i>{{ item.resource.owner }}</span>
            </td>
            <td v-if="item.control.id === 'container_image.CVE_scan' && item.control.message !== 'No issues'" :style="getColumnWidth(5)">
              <router-link class="small-link" :to="{ name: 'ImageDetail', params: { imageid: item.tags.imageTag, date: date, component: item.component }}">{{ item.control.id }}</router-link>
              <span v-if="item.tags.imageTag" class="table-tooltip" data-balloon-length="large" data-balloon-pos="up" :aria-label="item.tags.imageTag">
                <span class="icon-target tip-icon" style="font-size:9px;" />
              </span>
              <span v-if="item.control.message" class="table-tooltip" data-balloon-length="large" data-balloon-pos="up" :aria-label="item.control.message">
                <span class="icon-help-circle tip-icon" style="font-size:9px;"></span>
              </span>
            </td>
            <td v-else :style="getColumnWidth(5)">
              <router-link class="small-link" :to="{ name: 'CheckDetail', params: { checkid: item.control.id, date: date, component: item.component }}">{{ item.control.id }}</router-link>
            </td>
            <td :class="`result${item.result}`" :style="getColumnWidth(6)" class="row-ellipsis">{{ item.result }}</td>
          </tr>
        </tbody>
      </table>
      <div class="table-footer">
        <Paginator
          :pageIndex="data.pageIndex"
          :pageSize="pageSize"
          :totalRows="data.totalResults"
          @pageChanged="changePageIndex"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts" src="./OverviewDetail.ts"></script>
<style lang="scss" src="./OverviewDetail.scss"></style>