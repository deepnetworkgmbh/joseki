<template>
    <div class="segment breadcrumbs">
        <p style="float:left">
            <a class="link" href="/"><i class="icon-home" /> Home</a>
        </p>
        <p v-if="date && showDate" style="float:left">
            <span class="slash">/</span>      
            <a class="link" :href="`/overview/${date}`"><i class="icon-calendar" /> {{date}}</a>
        </p>
        <p v-if="showComponent && component.name.length>0" style="float:left">
            <span class="slash">/</span>
            <a class="link" :href="getComponentLink()"><i :class="getIcon()" /> {{ getComponentLabel() }}</a>
        </p>
        <p v-if="showHistory" style="float:left">
            <span class="slash">/</span>
            Scan History
        </p>
        <p v-if="showDiff" style="float:left">
            <span class="slash">/</span>
            Compare Scans
            <span class="slash">/</span>
            {{ diffdates }}
        </p>
        <p v-if="showImage" style="float:left">
            <span class="slash">/</span>
            <span><i class="icon-target" /> Image: {{ imageid }}</span>
        </p>
        <p >
            <span class="slash">/</span>
        </p>
        <AdvancedFilter v-if="showFilter" style="float:left" :filter="filter" @filterUpdated="onFilterChangedFromAF" />
    </div>
</template>
<script lang="ts" src="./Breadcrumbs.ts"></script>
<style scoped lang="scss" src="./Breadcrumbs.scss"></style>
