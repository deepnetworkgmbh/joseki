<template>
  <div id="nav">
    <div v-if="authEnabled()">
      <button class="btn btn-mini btn-signout" @click="signout()">Sign Out</button>
      <div style="float:right">
        <p class="username">{{ user.name }}</p>
        <p class="useremail">{{ user.email }}</p>
      </div>
    </div>
    <div class="logo" style="cursor:pointer;">
      <router-link to="/">
        <span class="icon-shield" style="font-size:19px;"></span>Joseki
      </router-link>
    </div>    
  </div>
</template>
<script lang="ts" src="./Navigation.ts"></script>
<style scoped lang="scss" src="./Navigation.scss"></style>
