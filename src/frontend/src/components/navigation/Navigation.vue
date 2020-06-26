<template>
  <div id="nav">
    <div>
      <button class="btn btn-mini" @click="signout()" style="float:right;">Sign Out</button>
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
