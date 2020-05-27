<template>
  <div v-if="filterContainer" class="advanced-filter noselect">
    <div class="advanced-filter-menu">
        <div class="advanced-filter-container">            
            <div v-for="(filter,i) in filterContainer.filters" :key="`filter${i}`">
                <div :id="`edit-filter-button-${i}`" class="advanced-filter-tag gradient">
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
                    <div v-if="filter.deletable" style="z-index:1000;margin-top:1px;float:left;"><i class="icon-x-circle advanced-filter-delete-tag" @click="deleteFilter(i)" /></div>
                </div>               
            </div>            
            <div v-if="filterContainer.filters.length===0" style="float:left;margin-right:2px;">
              Displaying all scan results.
            </div>
            <div v-if="filterContainer.filters.length<6" class="advanced-filter-tag gradient">
              <span id="add-filter-button" class="tag-hover-clickable" @click="showMenuInAddMode()">Add Filter</span>
            </div>
            <div v-if="showAddMenu" :class="getAddMenuClass()" :style="{ left: addMenuX + 'px' }">
              <!-- <i class="icon-x-circle advanced-filter-delete-tag" @click="showAddMenu = false" style="position:absolute;right:3px;top:1px" /> -->
              <div v-if="showAddMenu" style="width:90px;float:left;margin-right:3px;">
                <div class="adv-filter-checks adv-filter-checks-left">
                    <ul style="padding:0;text-align:left;font-size:11px;">
                        <li v-for="(option,j) in addFilterTypes" :key="`opt${j}`" style="padding:2px" @click="changeFilterType(option)" :class="getFilterTypeClass(option)">
                            <label>{{ option }}</label>
                        </li>
                    </ul>
                </div>
              </div>
              <div v-if="showAddMenu" class="adv-filter-checks adv-filter-checks-right">
                <div v-if="addFilterValues.length>0" style="border-bottom:solid 1px #aaa;">
                  <div v-if="selectedFilterType!=='component' && channel==='overview'" style="width:80px;height:20px;position:absolute;right:0;font-size:9px;cursor:pointer;padding-top:4px;" @click="onlyWithValues = !onlyWithValues">
                    <i :class="onlyWithValues ? 'icon-toggle-right' : 'icon-toggle-left'" :style="{ color: onlyWithValues ? '#080' : '#888'}" />
                    Only with values
                  </div>
                  <i class="icon-search" style="color:#ccc;padding-left:4px;padding-top:1px;font-size:9px" />
                  <input type="text" class="filter-value-filter-input" placeholder="type to search for values" v-model="filteredValueFilter" />
                </div>
                <div style="height:142px;overflow-y: auto;margin-right:2px;margin-top:2px;">                    
                    <ul style="padding:0;text-align:left;font-size:10px;margin-left:2px;">
                        <li v-for="(option,j) in getFilteredFilterValues()" :key="`opt${j}`" @click.stop="toggleFilterValueChecked(j)">
                            <input type="checkbox" :checked="option.checked" />
                            <span v-html="getHighlightedText(option.label)" style="margin-left:2px"></span>
                            <i v-if="!option.label" style="color:#777;">undefined </i>
                            <span v-if="selectedFilterType!=='component'" style="color:#777">({{ option.count }})</span>
                        </li>
                        <li v-if="addFilterValues.length===0" style="padding:5px;">Please make a selection</li>
                    </ul>
                </div>
              </div>
              <div v-if="showAddMenu" style="margin-top:170px;">
                <div style="float:left;font-size:10px;padding:5px">
                  {{ addFilterSelectionCount }} selected
                </div>
                <button v-if="mode === 'add'" class="btn-xxs filter-form-button gradient" :disabled="!addFilterButtonEnabled" @click="addSelectionToFilters()">
                  Add Filter
                </button>
                <button v-if="mode === 'edit'" class="btn-xxs filter-form-button gradient" @click="updateFiltersSelection()">
                  Update
                </button>
                <button class="btn-xxs filter-form-button gradient" @click="hideAddMenu()">
                  Cancel
                </button>
              </div>
            </div>           
        </div>
    </div>
  </div>
</template>
<script lang="ts" src="./AdvancedFilter.ts"></script>
<style scoped lang="scss" src="./AdvancedFilter.scss"></style>
