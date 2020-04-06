<template>
    <div class="w-full flex spinner-container">
        <div v-if="!loadFailed" class="lds-roller justify-center align-middle">
            <div></div>
            <div></div>
            <div></div>
            <div></div>
            <div></div>
            <div></div>
            <div></div>
            <div></div>
        </div>
        <div v-if="loadFailed" class="load-failed" @click='reloadClicked()'>
            <i class="icon-alert-triangle" style="color:#fff" /> Failed to load data from API, click here to refresh
        </div>
    </div>
</template>
<script lang="ts" src="./Spinner.ts"></script>
<style scoped lang="scss" src="./Spinner.scss"></style>
