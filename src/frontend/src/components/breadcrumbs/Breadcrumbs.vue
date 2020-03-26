<template>
    <div class="segment breadcrumbs">
        <a class="link" href="/"><i class="icon-home" /> Home</a> 
        <p v-if="date && showDate">
            <span class="slash">/</span>      
            <a class="link" :href="`/overview/${date}`"><i class="icon-calendar" /> {{date}}</a>
        </p>
        <p v-if="showComponent && component.name.length>0">
            <span class="slash">/</span>
            <a class="link" :href="getComponentLink()"><i :class="getIcon()" /> {{ getComponentLabel() }}</a>
        </p>
        <p v-if="showHistory">
            <span class="slash">/</span>
            Scan History
        </p>
        <p v-if="showDiff">
            <span class="slash">/</span>
            Compare Scans
            <span class="slash">/</span>
            {{ diffdates }}
        </p>
        <p v-if="showImage">
            <span class="slash">/</span>
            <span><i class="icon-target" /> Image: {{ imageid }}</span>
        </p>
    </div>
</template>
<script lang="ts" src="./Breadcrumbs.ts"></script>
<style scoped lang="scss" src="./Breadcrumbs.scss"></style>
