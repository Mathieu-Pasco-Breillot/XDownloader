<template>
  <v-app dark>
    <v-toolbar app></v-toolbar>
    <v-content>
      <v-container fluid>
        <router-view/>
      </v-container>
    </v-content>
  </v-app>
</template>

<style>
  .center {
    display: block;
    margin-left: auto;
    margin-right: auto;
  }
</style>
