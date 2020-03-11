<template>
    <li class="control-li text-sm">
        <i :class="control.icon"></i>
        {{ control.id }}
        <span class="ml-1 mr-1"
            data-balloon-length="xlarge"
            data-balloon-pos="up"
            :aria-label="control.text">
            <i class="far fa-question-circle tip-icon"></i>
        </span>
        <span v-if="control.id === 'container_image.CVE_scan' && control.text !== 'No issues'">
            <a class="small-link" @click="goToImageScan(control.tags.imageTag)">see details</a>
        </span>
    </li>
</template>
<script lang="ts" src="./ControlList.ts"></script>
<style scoped lang="scss" src="./ControlList.scss"></style>
