<template>
    <div style="width:100%;height:100vh;background-color:#eee;margin:auto;flex-direction: column;text-align:center;display:flex;justify-content: center;align-items:center;">
        <div style="">
            <h1>Joseki</h1>
            <button v-if="!isAuthenticated()" class="btn" @click="login()">login</button>
            <button v-if="isAuthenticated()" class="btn" @click="logout()">logout</button>
        </div>
    </div>
</template>
<script lang="ts" src="./HomeComponent.ts"></script>
<style lang="scss" src="./HomeComponent.scss"></style>