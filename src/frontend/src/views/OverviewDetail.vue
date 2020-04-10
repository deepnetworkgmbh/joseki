<template>
    <div>
        <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
        <div v-if="loaded" class="shadow" style="min-height:300px;padding:4px">
           <table class="table">
                <thead style="width:100%">
                    <tr class="table-header">
                        <td v-for="(col,i) in headers" :key="`col${i}`" :style="getColumnWidth(i)">
                            <span class="filter-button noselect" @click="toggleColumnFilter(i)"><i class="icon-filter" />
                                <span v-if="col.checkedCount()>0">({{ col.checkedCount() }})</span>
                            </span>
                            <span style="cursor:pointer" class="noselect" @click="changeOrdering(i)">
                                {{ col.label }}
                                <i v-if="col.sortable" :class="getHeaderClass(i)" />         
                            </span>
                            <div v-if="col.optionsMenuShown" class="filter-checks" @mouseleave="col.optionsMenuShown=false">
                                <ul style="padding:0">
                                    <li v-for="(option,j) in col.options" :key="`opt${i}-${j}`" style="padding:0" :class="option.dimmed ? 'option-dimmed': ''">
                                        <label><input type="checkbox" v-model="option.checked" @change.stop="toggleFilterSelection(i, j)" /> {{ option.label }}</label>
                                    </li>
                                </ul>
                            </div>
                        </td>
                        <AdvancedFilter :filter="filter" @filterUpdated="onFilterUpdated" />
                    </tr>
                </thead>
                <tbody style="width:100%">
                    <tr v-for="(item,i) in data.checks" :key="`item${i}`" class="checkrow">                    
                        <td>{{ item.category }}</td>
                        <td><i :class="getComponentIcon(item.component.category)" /> {{ item.component.name }}</td>
                        <td>{{ item.collection.type }}:{{ item.collection.name }}</td>
                        <td v-if="item.control.id === 'container_image.CVE_scan' && item.control.message !== 'No issues'">
                            <router-link class='small-link' :to="{ name: 'ImageDetail', params: { imageid: item.tags.imageTag, date: date, component: item.component }}">
                                {{ item.control.id }}</router-link>                            
                            <span v-if="item.control.message" class="table-tooltip" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="item.control.message">
                            <span class="icon-help-circle tip-icon" style="font-size:9px;"></span>
                            </span>
                        </td>
                        <td v-else>
                            {{ item.control.id }}
                            <span v-if="item.control.message" class="table-tooltip" data-balloon-length="xlarge" data-balloon-pos="up" :aria-label="item.control.message">
                            <span class="icon-help-circle tip-icon" style="font-size:9px;"></span>
                            </span>
                        </td>
                        <td>{{ item.resource.type }}:{{ item.resource.name }}</td>                    
                        <td :class='getResultClass(item.result)'>{{ item.result }}</td>                   
                    </tr> 
                </tbody>
            </table>
            <div class="table-footer">                    
                <Paginator :pageIndex="data.pageIndex" :pageSize="pageSize" :totalRows="data.totalResults" @pageChanged="changePageIndex" />
            </div>
        </div>
    </div>
</template>

<script lang="ts" src="./OverviewDetail.ts"></script>
<style lang="scss" src="./OverviewDetail.scss"></style>