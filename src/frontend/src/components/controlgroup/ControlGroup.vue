<template>
     <li style="padding:2px;padding-left:0;margin-left:5px;margin-top:0px;margin-bottom:2px;">
        <b>{{ name }} ({{ items.length }})</b>  
        <ul v-for="(control, cgi) in items" :key="`cgi${name}-${cgi}`">
            <ControlList :date="date" :control="control" />
        </ul>
    </li>
</template>
<script lang="ts" src="./ControlGroup.ts"></script>
<style scoped lang="scss" src="./ControlGroup.scss"></style>
