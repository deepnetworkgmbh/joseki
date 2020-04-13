<template>
    <li :class="getLineClass()">
        <span style="float:left; background-color:#fc0;border-radius:5px;padding-left:5px;padding-right:5px">
            <i :class="control.icon" style="padding-right:5px;"></i> {{ control.result }}
        </span>
        <span style="float:left"> : </span>
        <div :class='getIdClass()'>{{ control.id }}</div>
        <span class="ml-1 mr-1"
            data-balloon-length="xlarge"
            data-balloon-pos="up"
            :aria-label="control.text">
            <span class="icon-help-circle tip-icon"></span>
        </span>
        <span v-if="control.id === 'container_image.CVE_scan' && control.text !== 'No issues'">
            <a class="small-link" :href="imageScanUrl">see details</a>
        </span>
    </li>
</template>
<script lang="ts" src="./ControlList.ts"></script>
<style scoped lang="scss" src="./ControlList.scss"></style>
