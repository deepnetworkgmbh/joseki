<template>
  <div v-if="filterContainer" class="advanced-filter noselect">
    <div class="advanced-filter-menu">
        <div class="advanced-filter-container">            
            <div v-for="(filter,i) in filterContainer.filters" :key="`filter${i}`">
                <div class="advanced-filter-tag" style="cursor:pointer;">
                    <!-- <span v-if="menuOpen" class="icon-git-commit" style="float:left;margin:3px;background-color:#ccc;color:#fff;"></span> -->
                    <div @click="showMenuInEditMode(i)" style="float:left;">
                      <span style="padding:2px;font-weight:bold;color:#222;"> {{ filter.label }} </span> 
                      <span style="padding:2px;">=</span>
                      
                      <span v-if="filter.values.length === 1 && filter.values[0].length < 30">
                        <span v-for="(value,k) in filter.values" :key="`value-${i}-${k}`">
                          <span style="margin-right:1px;padding-left:2px;padding-right:2px;color:#222;">{{ value }}</span>
                          <span v-if="k<filter.values.length-1" style="color:#99a"> or </span>
                        </span>
                      </span>
                      <span v-else>
                        {{ filter.values.length }} selected
                      </span>                
                    </div>
                    <div style="z-index:1000;margin-top:1px;float:left;"><i class="icon-x-circle advanced-filter-delete-tag" @click="deleteFilter(i)" /></div>
                </div>               
            </div>            
            <div v-if="filterContainer.filters.length===0" style="float:left;margin-right:2px;">
              No active filter.
            </div>
            <div class="advanced-filter-tag">
              <span class="tag-hover-clickable" @click="showMenuInAddMode()">Add Filter</span>
              <div v-if="showAddMenu" :class="getAddMenuClass()">
                <!-- <i class="icon-x-circle advanced-filter-delete-tag" @click="showAddMenu = false" style="position:absolute;right:3px;top:1px" /> -->
                <div v-if="showAddMenu" style="width:90px;float:left;margin-right:3px;">
                  <div class="adv-filter-checks adv-filter-checks-left">
                      <ul style="padding:0;text-align:left">
                          <li v-for="(option,j) in addFilterTypes" :key="`opt${j}`" style="padding:2px" @click="changeFilterType(option)" :class="option===selectedFilterType ? 'adv-filter-selected' : ''">
                              <label>{{ option }}</label>
                          </li>
                      </ul>
                  </div>
                </div>
                <div v-if="showAddMenu" class="adv-filter-checks adv-filter-checks-right">
                  <div style="height:166px;overflow-y: auto;margin-right:2px;margin-top:1px;">
                      <ul style="padding:0;text-align:left;">
                          <li v-for="(option,j) in addFilterValues" :key="`opt${j}`" style="padding:2px" @click.stop="toggleFilterValueChecked(j)">
                              <label>
                                <input type="checkbox" :checked="option.checked" @change="toggleFilterValueChecked(j)"  /> {{ option.label }} 
                                <span v-if="selectedFilterType!=='component'">({{ option.count }})</span>
                              </label>
                          </li>
                          <li v-if="addFilterValues.length===0" style="padding:5px;">Please make a selection</li>
                      </ul>
                  </div>
                </div>
                <div v-if="showAddMenu" style="margin-top:168px;">
                  <button v-if="mode === 'add'" class="btn-xxs filter-form-button" :disabled="!addFilterButtonEnabled" @click="addSelectionToFilters()">
                    Add Filter
                  </button>
                  <button v-if="mode === 'edit'" class="btn-xxs filter-form-button" @click="updateFiltersSelection()">
                    Update
                  </button>
                  <button class="btn-xxs filter-form-button" @click="showAddMenu = false">
                    Cancel
                  </button>
                </div>
              </div>
            </div>
        </div>
    </div>
  </div>
</template>
<script lang="ts" src="./AdvancedFilter.ts"></script>
<style scoped lang="scss" src="./AdvancedFilter.scss"></style>
