<template>
    <div class="landing-container">
        <div style="">
            <h1 style="font-size:19px;margin-bottom:10px;"><span class="icon-shield" style="font-size:19px;"></span>Joseki</h1>
            <div v-if="hasNoRole" class="landing-no-roles-assigned">
                You don't have any roles assigned.<br>
                Please contact your Joseki Administrator and re-login.<br>
            </div>
            <button v-if="!isAuthenticated()" class="btn" @click="login()">login</button>
            <button v-if="isAuthenticated()" class="btn" @click="logout()">logout</button>
        </div>
    </div>
</template>
<script lang="ts" src="./HomeComponent.ts"></script>
<style lang="scss" src="./HomeComponent.scss"></style>
